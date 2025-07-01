using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SpaceWarp.API.Backend.SaveGameManager;

namespace SpaceWarp.API.SaveGameManager;

/// <summary>
/// This class allows you to register your mod data for the game's save file system .
/// </summary>
[PublicAPI]
public static class ModSaves
{
    /// <summary>
    /// Per-save plugin data
    /// </summary>
    public static List<PluginSaveData> PluginSaveData = new();
    /// <summary>
    /// Per-campaign plugin data
    /// </summary>
    public static List<PluginSaveData> PluginCampaignData = new();

    /// <summary>
    /// Registers your mod data for saving and loading events.
    /// </summary>
    /// <typeparam name="T">Any object</typeparam>
    /// <param name="modGuid">
    /// Your mod GUID. Or, technically, any kind of string can be passed here, but what is mandatory is that it's unique
    /// compared to what other mods will use.
    /// </param>
    /// <param name="onSave">
    /// Function that will execute when a SAVE event is triggered. Defaults to null or no callback.
    /// </param>
    /// <param name="onLoad">
    /// Function that will execute when a LOAD event is triggered. Defaults to null or no callback.
    /// </param>
    /// <param name="saveData">
    /// Your object that will be saved to a save file during a save event and that will be updated when a load event
    /// pulls new data. Ensure that a new instance of this object is NOT created after registration.
    /// </param>
    /// <param name="persistenceKind">
    /// How the save data is persisted, per save or per campaign, your mod can register save data in one of each of these
    /// slots
    /// </param>
    /// <returns>
    /// T saveData object you passed as a parameter, or a default instance of object T if you didn't pass anything
    /// </returns>
    public static T RegisterSaveLoadGameData<T>(
        string modGuid,
        Action<T>? onSave = null,
        Action<T>? onLoad = null,
        T? saveData = null,
        PersistenceKind persistenceKind = PersistenceKind.PerSave
    ) where T : class
    {
        var saveDataList = persistenceKind == PersistenceKind.PerSave ? PluginSaveData : PluginCampaignData;
        // Check if this GUID is already registered
        if (saveDataList.Find(p => p.ModGuid == modGuid) != null)
        {
            throw new ArgumentException($"Mod GUID '{modGuid}' is already registered. Skipping.", nameof(modGuid));
        }

        saveData ??= Activator.CreateInstance<T>();

        saveDataList.Add(new PluginSaveData
        {
            ModGuid = modGuid,
            SaveEventCallback = SaveCallbackAdapter,
            LoadEventCallback = LoadCallbackAdapter,
            SaveData = saveData
        });
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"Registered '{modGuid}' for {persistenceKind.ToPersistenceString()} save/load events.");
        return saveData;

        // Create adapter functions to convert Action<T> to CallbackFunctionDelegate

        void LoadCallbackAdapter(object dataToBeLoaded)
        {
            if (onLoad != null && dataToBeLoaded is T data)
            {
                onLoad(data);
            }
        }

        void SaveCallbackAdapter(object dataToBeSaved)
        {
            if (onSave != null && dataToBeSaved is T data)
            {
                onSave(data);
            }
        }
    }

    /// <summary>
    /// Unregister your previously registered mod data for saving and loading. Use this if you no longer need your data
    /// to be saved and loaded.
    /// </summary>
    /// <param name="modGuid">Your mod GUID you used when registering.</param>
    /// <param name="persistenceKind">
    /// The type of persistence you used when registering.
    /// </param>
    public static void UnRegisterSaveLoadGameData(string modGuid, PersistenceKind persistenceKind = PersistenceKind.PerSave)
    {
        var saveDataList = persistenceKind == PersistenceKind.PerSave ? PluginSaveData : PluginCampaignData;
        var toRemove = saveDataList.Find(p => p.ModGuid == modGuid);
        if (toRemove == null) return;
        saveDataList.Remove(toRemove);
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"Unregistered '{modGuid}' for {persistenceKind.ToPersistenceString()} save/load events.");
    }

    /// <summary>
    /// Unregisters then again registers your mod data for saving and loading events
    /// </summary>
    /// <typeparam name="T">The type of your save data</typeparam>
    /// <param name="modGuid">
    /// Your mod GUID. Or, technically, any kind of string can be passed here, but what is mandatory is that it's unique
    /// compared to what other mods will use.
    /// </param>
    /// <param name="onSave">
    /// Function that will execute when a SAVE event is triggered. Defaults to null or no callback.
    /// </param>
    /// <param name="onLoad">
    /// Function that will execute when a LOAD event is triggered. Defaults to null or no callback.
    /// </param>
    /// <param name="saveData">
    /// Your object that will be saved to a save file during a save event and that will be
    /// updated when a load event pulls new data. Ensure that a new instance of this object is NOT created after
    /// registration.
    /// </param>
    /// <param name="persistenceKind">
    /// How the save data is persisted, per save or per campaign, your mod can register save data in one of each of these
    /// slots
    /// </param>
    /// <returns>
    /// T saveData object you passed as a parameter, or a default instance of object T if you didn't pass anything
    /// </returns>
    public static T ReregisterSaveLoadGameData<T>(
        string modGuid,
        Action<T>? onSave = null,
        Action<T>? onLoad = null,
        T? saveData = null,
        PersistenceKind persistenceKind = PersistenceKind.PerSave
    ) where T : class
    {
        UnRegisterSaveLoadGameData(modGuid);
        return RegisterSaveLoadGameData(modGuid, onSave, onLoad, saveData);
    }
    
}