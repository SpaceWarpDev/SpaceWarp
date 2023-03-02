global using UnityObject = UnityEngine.Object;
global using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using KSP.Messages;
using SpaceWarp.API.Game;
using SpaceWarp.API.Mods;
using SpaceWarp.UI;
using UnityEngine;

namespace SpaceWarp;

[BepInPlugin(ModGuid, ModName, ModVer)]
public sealed class SpaceWarpPlugin : BaseSpaceWarpPlugin
{
    public const string ModGuid = "com.github.x606.spacewarp";
    public const string ModName = "Space Warp";
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    new internal ManualLogSource Logger => base.Logger;

    public void Awake()
    {
        BepInEx.Logging.Logger.Listeners.Add(new SpaceWarpConsoleLogListener());

        Harmony.CreateAndPatchAll(typeof(SpaceWarpPlugin).Assembly, ModGuid);
        
        SpaceWarpManager.Initialize(this);
    }


    public override void OnInitialized()
    {
        base.OnInitialized();
        
        Game.Messages.Subscribe(typeof(GameStateEnteredMessage), StateChanges.OnGameStateEntered,false,true);
        Game.Messages.Subscribe(typeof(GameStateLeftMessage), StateChanges.OnGameStateLeft,false,true);
        
        InitializeUI();
    }

    private void InitializeUI()
    {
        GameObject modUIObject = new("Space Warp Mod UI");
        modUIObject.Persist();

        modUIObject.transform.SetParent(this.transform);
        SpaceWarpManager.ModListUI = modUIObject.AddComponent<ModListUI>();

        modUIObject.SetActive(true);

        GameObject consoleUIObject = new("Space Warp Console");
        consoleUIObject.Persist();
        consoleUIObject.transform.SetParent(Chainloader.ManagerObject.transform);
        consoleUIObject.AddComponent<SpaceWarpConsole>();
        consoleUIObject.SetActive(true);
        
        API.UI.MainMenu.RegisterMenuButton("mods", SpaceWarpManager.ModListUI.ToggleVisible);
    }
}