using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace SpaceWarp.Backend.SaveGameManager;

/// <summary>
/// Extension of game's save/load data class
/// </summary>
[Serializable]
public class SpaceWarpSerializedSavedGame : KSP.Sim.SerializedSavedGame
{
    public List<PluginSaveData> serializedPluginSaveData = new();
}
