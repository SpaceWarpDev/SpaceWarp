using System;
using System.Collections.Generic;

namespace SpaceWarp.Backend.SaveGameManager;

/// <summary>
/// Extension of game's save/load data class
/// </summary>
[Serializable]
public class SpaceWarpSerializedSavedGame : KSP.Sim.SerializedSavedGame
{
    public List<PluginSaveData> SerializedPluginSaveData = new();
}
