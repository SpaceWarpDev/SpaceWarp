using JetBrains.Annotations;
using SpaceWarp.Backend.SaveGameManager;
using System;
using System.Collections.Generic;
using SpaceWarp.API.Logging;

namespace SpaceWarp.API.SaveGameManager;

[PublicAPI]
public static class ModSaves
{
    private static readonly ILogger _logger = new UnityLogSource("SpaceWarp.ModSaves");

    internal static List<PluginSaveData> InternalPluginSaveData = new();

    /// <summary>
    /// Registers your mod data for saving and loading events.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="modGuid">Your mod GUID. Or, technically, any kind of string can be passed here, but what is mandatory is that it's unique compared to what other mods will use.</param>
    /// <param name="saveData">Your object that will be saved to a save file during a save event and that will be updated when a load event pulls new data. Ensure that a new instance of this object is NOT created after registration.</param>
    /// <param name="saveEventCallback">Function that will execute when a SAVE event is triggered. 'NULL' is also valid here if you don't need a callback.</param>
    /// <param name="loadEventCallback">Function that will execute when a LOAD event is triggered. 'NULL' is also valid here if you don't need a callback.</param>
    public static void RegisterSaveLoadGameData<T>(string modGuid, T saveData, Action<T> saveEventCallback, Action<T> loadEventCallback)
    {
        // Create adapter functions to convert Action<T> to CallbackFunctionDelegate
        SaveGameCallbackFunctionDelegate saveCallbackAdapter = (object saveData) =>
        {
            if (saveEventCallback != null && saveData is T data)
            {
                saveEventCallback(data);
            }
        };

        SaveGameCallbackFunctionDelegate loadCallbackAdapter = (object saveData) =>
        {
            if (loadEventCallback != null && saveData is T data)
            {
                loadEventCallback(data);
            }
        };

        // Check if this GUID is already registered
        if (InternalPluginSaveData.Find(p => p.ModGuid == modGuid) != null)
        {
            throw new ArgumentException($"Mod GUID '{modGuid}' is already registered. Skipping.", "modGuid");
        }
        else
        {
            InternalPluginSaveData.Add(new PluginSaveData { ModGuid = modGuid, SaveData = saveData, SaveEventCallback = saveCallbackAdapter, LoadEventCallback = loadCallbackAdapter });
            _logger.LogInfo($"Registered '{modGuid}' for save/load events.");
        }
    }

    /// <summary>
    /// Unregister your previously registered mod data for saving and loading. Use this if you no longer need your data to be saved and loaded.
    /// </summary>
    /// <param name="modGuid">Your mod GUID you used when registering.</param>
    public static void UnRegisterSaveLoadGameData(string modGuid)
    {
        var toRemove = InternalPluginSaveData.Find(p => p.ModGuid == modGuid);
        if (toRemove != null)
        {
            InternalPluginSaveData.Remove(toRemove);
            _logger.LogInfo($"Unregistered '{modGuid}' for save/load events.");
        }
    }

    /// <summary>
    /// Unregisters then again registers your mod data for saving and loading events
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="modGuid">Your mod GUID. Or, technically, any kind of string can be passed here, but what is mandatory is that it's unique compared to what other mods will use.</param>
    /// <param name="saveData">Your object that will be saved to a save file during a save event and that will be updated when a load event pulls new data. Ensure that a new instance of this object is NOT created after registration.</param>
    /// <param name="saveEventCallback">Function that will execute when a SAVE event is triggered. 'NULL' is also valid here if you don't need a callback.</param>
    /// <param name="loadEventCallback">Function that will execute when a LOAD event is triggered. 'NULL' is also valid here if you don't need a callback.</param>
    public static void ReregisterSaveLoadGameData<T>(string modGuid, T saveData, Action<T> saveEventCallback, Action<T> loadEventCallback)
    {
        UnRegisterSaveLoadGameData(modGuid);
        RegisterSaveLoadGameData<T>(modGuid, saveData, saveEventCallback, loadEventCallback);
    }
}
