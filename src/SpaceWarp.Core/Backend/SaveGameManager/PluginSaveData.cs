namespace SpaceWarp.Backend.SaveGameManager;

/// <summary>
/// The delegate type that will be called when a save or load event is triggered.
/// </summary>
public delegate void SaveGameCallbackFunctionDelegate(object data);

/// <summary>
/// This class is used to store your mod data for saving and loading.
/// </summary>
[Serializable]
public class PluginSaveData
{
    /// <summary>
    /// The GUID of your mod
    /// </summary>
    public string ModGuid { get; set; }

    /// <summary>
    /// The data that will be saved
    /// </summary>
    public object SaveData { get; set; }

    [NonSerialized]
    internal SaveGameCallbackFunctionDelegate SaveEventCallback;
    [NonSerialized]
    internal SaveGameCallbackFunctionDelegate LoadEventCallback;
}
