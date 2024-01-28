using HarmonyLib;
using KSP.Game;
using KSP.Game.Load;
using KSP.IO;
using Newtonsoft.Json;
using SpaceWarp.API.SaveGameManager;
using SpaceWarp.Backend.SaveGameManager;
using SpaceWarp.InternalUtilities;

namespace SpaceWarp.Patching.SaveGameManager;

[HarmonyPatch]
internal class SaveLoadPatches
{
    #region Saving
    
    /// <summary>
    /// Common method used before serialization to save plugin data, if any.
    /// </summary>
    private static void SavePluginData(LoadGameData data)
    {
        // Take the game's LoadGameData, extend it with our own class and copy plugin save data to it
        SpaceWarpSerializedSavedGame modSaveData = new();
        InternalExtensions.CopyFieldAndPropertyDataFromSourceToTargetObject(data.SavedGame, modSaveData);
        modSaveData.serializedPluginSaveData = ModSaves.InternalPluginSaveData;
        data.SavedGame = modSaveData;

        // Initiate save callback for plugins that specified a callback function
        foreach (var plugin in ModSaves.InternalPluginSaveData)
        {
            plugin.SaveEventCallback(plugin.SaveData);
        }
    }

    [HarmonyPatch(typeof(SerializeGameDataFlowAction), MethodType.Constructor, [typeof(string), typeof(LoadGameData)])]
    [HarmonyPostfix]
    private static void InjectPluginSaveGameData(
        string filename,
        LoadGameData data,
        // ReSharper disable once InconsistentNaming
        SerializeGameDataFlowAction __instance
    )
    {
        // Skip plugin data injection if there are no mods that have registered for save/load actions
        if (ModSaves.InternalPluginSaveData.Count == 0)
        {
            return;
        }

        SavePluginData(data);
    }
    
    /// <summary>
    /// Handles save game serialization in memory, like when launching from VAB. Current
    /// game is serialized to a buffer and kept in memory.
    /// </summary>
    [HarmonyPatch(typeof(SerializeGameToMemoryFlowAction), MethodType.Constructor, [typeof(LoadOrSaveCampaignTicket)])]
    [HarmonyPostfix]
    private static void InjectToMemoryPluginSaveGameData(
        LoadOrSaveCampaignTicket loadOrSaveCampaignTicket,
        // ReSharper disable once InconsistentNaming
        SerializeGameToMemoryFlowAction __instance
    )
    {
        // Skip plugin data injection if there are no mods that have registered for save/load actions
        if (ModSaves.InternalPluginSaveData.Count == 0)
        {
            return;
        }
        
        SavePluginData(loadOrSaveCampaignTicket.LoadGameData);
    }

    #endregion

    #region Loading

    /// <summary>
    /// Common method used after deserialization to load plugin data, if any.
    /// </summary>
    private static void LoadPluginSaveData(SpaceWarpSerializedSavedGame serializedSavedGame)
    {
        // Perform plugin load data if plugin data is found in the save file
        if (serializedSavedGame.serializedPluginSaveData.Count <= 0) return;
        
        // Iterate through each plugin
        foreach (var loadedData in serializedSavedGame.serializedPluginSaveData)
        {
            // Match registered plugin GUID with the GUID found in the save file
            var existingData = ModSaves.InternalPluginSaveData.Find(
                p => p.ModGuid == loadedData.ModGuid
            );
            if (existingData == null)
            {
                SpaceWarpPlugin.Instance.SWLogger.LogWarning(
                    $"Saved data for plugin '{loadedData.ModGuid}' found during a load event, however " +
                    $"that plugin isn't registered for save/load events. Skipping load for this plugin."
                );
                continue;
            }

            // Perform a callback if plugin specified a callback function. This is done before plugin data is
            // actually updated.
            existingData.LoadEventCallback(loadedData.SaveData);

            // Copy loaded data to the SaveData object plugin registered
            InternalExtensions.CopyFieldAndPropertyDataFromSourceToTargetObject(
                loadedData.SaveData,
                existingData.SaveData
            );
        }
    }
    
    /// <summary>
    /// DeserializeBufferFlowAction is used when reverting to VAB / Launch from flight
    /// </summary>
    [HarmonyPatch(typeof(DeserializeBufferFlowAction), "DoAction")]
    [HarmonyPrefix]
    private static bool DeserializeBufferLoadedPluginData(
        Action resolve,
        Action<string> reject,
        // ReSharper disable once InconsistentNaming
        DeserializeBufferFlowAction __instance
    )
    {
        // Skip plugin deserialization if there are no mods that have registered for save/load actions
        if (ModSaves.InternalPluginSaveData.Count == 0)
        {
            return true;
        }

        __instance._game.UI.SetLoadingBarText(__instance.Description);
        try
        {
            if (DeserializeBufferFlowAction._ignoreNullValueSerialzationSettings == null)
            {
                DeserializeBufferFlowAction._ignoreNullValueSerialzationSettings = IOProvider.CloneSerializerSettings(IOProvider.GetDefaultSerializerSettings());
                DeserializeBufferFlowAction._ignoreNullValueSerialzationSettings.NullValueHandling = NullValueHandling.Ignore;
            }
            
            // Deserialize save buffer to our own class that extends game's SerializedSavedGame
            var serializedSavedGame = IOProvider.FromBuffer<SpaceWarpSerializedSavedGame>(__instance._savedGameBuffer, DeserializeBufferFlowAction._ignoreNullValueSerialzationSettings);
            __instance._data.SavedGame = serializedSavedGame;
            __instance._data.DataLength = (long) __instance._savedGameBuffer.Length;

            // Perform plugin load data if plugin data is found in the save file
            LoadPluginSaveData(serializedSavedGame);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
            reject(ex.Message);
        }

        resolve();

        return false;
    }
    
    /// <summary>
    /// DeserializeContentsFlowAction is used when loading a save file
    /// </summary>
    [HarmonyPatch(typeof(DeserializeContentsFlowAction), "DoAction")]
    [HarmonyPrefix]
    private static bool DeserializeContentsLoadedPluginData(
        Action resolve,
        Action<string> reject,
        // ReSharper disable once InconsistentNaming
        DeserializeContentsFlowAction __instance
    )
    {
        // Skip plugin deserialization if there are no mods that have registered for save/load actions
        if (ModSaves.InternalPluginSaveData.Count == 0)
        {
            return true;
        }

        __instance._game.UI.SetLoadingBarText(__instance.Description);
        try
        {
            // Deserialize save data to our own class that extends game's SerializedSavedGame
            IOProvider.FromJsonFile<SpaceWarpSerializedSavedGame>(__instance._filename, out var serializedSavedGame);
            __instance._data.SavedGame = serializedSavedGame;
            __instance._data.DataLength = IOProvider.GetFileSize(__instance._filename);

            // Perform plugin load data if plugin data is found in the save file
            LoadPluginSaveData(serializedSavedGame);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
            reject(ex.Message);
        }

        resolve();

        return false;
    }

    #endregion
}