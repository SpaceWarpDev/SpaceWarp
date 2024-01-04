namespace SpaceWarp.Backend.SaveGameManager;

/// <summary>
/// Extension of game's save/load data class
/// </summary>
[Serializable]
public class SpaceWarpSerializedSavedGame : KSP.Sim.SerializedSavedGame
{
    /// <summary>
    /// List of serialized plugin save data
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public List<PluginSaveData> serializedPluginSaveData = new();
}
