using JetBrains.Annotations;
using SpaceWarp.Backend.SaveGameManager;
using SpaceWarp.API.Logging;

namespace SpaceWarp.API.SaveGameManager;

[PublicAPI]
public static class ModSaves
{
    private static readonly ILogger Logger = new UnityLogSource("SpaceWarp.ModSaves");

    internal static List<PluginSaveData> InternalPluginSaveData = new();

    /// <summary>
    /// Registers your mod data for saving and loading events.
    /// </summary>
    /// <typeparam name="T">Any object</typeparam>
    /// <param name="modGuid">Your mod GUID. Or, technically, any kind of string can be passed here, but what is mandatory is that it's unique compared to what other mods will use.</param>    
    /// <param name="onSave">Function that will execute when a SAVE event is triggered. Defaults to null or no callback.</param>
    /// <param name="onLoad">Function that will execute when a LOAD event is triggered. Defaults to null or no callback.</param>
    /// <param name="saveData">Your object that will be saved to a save file during a save event and that will be updated when a load event pulls new data. Ensure that a new instance of this object is NOT created after registration.</param>
    /// <returns>T saveData object you passed as a parameter, or a default instance of object T if you didn't pass anything</returns>
    public static T RegisterSaveLoadGameData<T>(string modGuid, Action<T> onSave = null, Action<T> onLoad = null, T saveData = default)
    {
        // Check if this GUID is already registered
        if (InternalPluginSaveData.Find(p => p.ModGuid == modGuid) != null)
        {
            throw new ArgumentException($"Mod GUID '{modGuid}' is already registered. Skipping.", nameof(modGuid));
        }

        saveData ??= Activator.CreateInstance<T>();

        InternalPluginSaveData.Add(new PluginSaveData { ModGuid = modGuid, SaveEventCallback = SaveCallbackAdapter, LoadEventCallback = LoadCallbackAdapter, SaveData = saveData });
        Logger.LogInfo($"Registered '{modGuid}' for save/load events.");
        return saveData;

        void LoadCallbackAdapter(object dataToBeLoaded)
        {
            if (onLoad != null && dataToBeLoaded is T data)
            {
                onLoad(data);
            }
        }

        // Create adapter functions to convert Action<T> to CallbackFunctionDelegate
        void SaveCallbackAdapter(object dataToBeSaved)
        {
            if (onSave != null && dataToBeSaved is T data)
            {
                onSave(data);
            }
        }
    }

    /// <summary>
    /// Unregister your previously registered mod data for saving and loading. Use this if you no longer need your data to be saved and loaded.
    /// </summary>
    /// <param name="modGuid">Your mod GUID you used when registering.</param>
    public static void UnRegisterSaveLoadGameData(string modGuid)
    {
        var toRemove = InternalPluginSaveData.Find(p => p.ModGuid == modGuid);
        if (toRemove == null) return;
        InternalPluginSaveData.Remove(toRemove);
        Logger.LogInfo($"Unregistered '{modGuid}' for save/load events.");
    }

    /// <summary>
    /// Unregisters then again registers your mod data for saving and loading events
    /// </summary>
    /// <typeparam name="T"> Any object</typeparam>
    /// <param name="modGuid">Your mod GUID. Or, technically, any kind of string can be passed here, but what is mandatory is that it's unique compared to what other mods will use.</param>    
    /// <param name="onSave">Function that will execute when a SAVE event is triggered. Defaults to null or no callback.</param>
    /// <param name="onLoad">Function that will execute when a LOAD event is triggered. Defaults to null or no callback.</param>
    /// <param name="saveData">Your object that will be saved to a save file during a save event and that will be updated when a load event pulls new data. Ensure that a new instance of this object is NOT created after registration.</param>
    /// <returns>T saveData object you passed as a parameter, or a default instance of object T if you didn't pass anything</returns>
    public static T ReregisterSaveLoadGameData<T>(string modGuid, Action<T> onSave = null, Action<T> onLoad = null, T saveData = default(T))
    {
        UnRegisterSaveLoadGameData(modGuid);
        return RegisterSaveLoadGameData(modGuid, onSave, onLoad, saveData);
    }
}
