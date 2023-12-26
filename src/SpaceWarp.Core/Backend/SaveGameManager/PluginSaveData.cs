namespace SpaceWarp.Backend.SaveGameManager;

internal delegate void SaveGameCallbackFunctionDelegate(object data);

[Serializable]
public class PluginSaveData
{
    public string ModGuid { get; set; }
    public object SaveData { get; set; }

    [NonSerialized]
    internal SaveGameCallbackFunctionDelegate SaveEventCallback;
    [NonSerialized]
    internal SaveGameCallbackFunctionDelegate LoadEventCallback;
}
