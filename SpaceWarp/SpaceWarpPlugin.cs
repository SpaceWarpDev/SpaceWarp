global using UnityObject = UnityEngine.Object;
global using System.Linq;
using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KSP.Game;
using KSP.Messages;
using KSP.VFX;
using SpaceWarp.UI;

namespace SpaceWarp;

[BepInPlugin(ModGuid, ModName, ModVer)]
public sealed class SpaceWarpPlugin : BaseUnityPlugin
{
    public const string ModGuid = "com.github.x606.spacewarp";
    public const string ModName = "Space Warp";
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    #region KspBehaviour things

    private GameInstance Game => GameManager.Instance == null ? null : GameManager.Instance.Game;

    internal MessageCenter Messages => Game.Messages;

    // ReSharper disable Unity.NoNullPropagation
    // fine because its null checked by Game properly
    private ContextualFxSystem CFXSystem => Game?.GraphicsManager?.ContextualFxSystem;

    private bool IsGameShuttingDown => Game == null;

    #endregion

    new internal ManualLogSource Logger => base.Logger;

    public void Awake()
    {
        BepInEx.Logging.Logger.Listeners.Add(new SpaceWarpConsoleLogListener());

        Harmony.CreateAndPatchAll(typeof(SpaceWarpPlugin).Assembly, ModGuid);
        
        SpaceWarpManager.Initialize(this);
    }

    

}