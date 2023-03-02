using System;
using BepInEx.Bootstrap;
using KSP.Game.Flow;
using SpaceWarp.UI;
using UnityEngine;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class InitializeSpaceWarpUIAction : FlowAction
{
    public InitializeSpaceWarpUIAction() : base("Loading Space Warp UI") { }
    
    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            GameObject modUIObject = new GameObject("Space Warp Mod UI");
            modUIObject.Persist();

            modUIObject.transform.SetParent(Chainloader.ManagerObject.transform);
            SpaceWarpManager.ModListUI = modUIObject.AddComponent<ModListUI>();

            modUIObject.SetActive(true);

            GameObject consoleUIObject = new GameObject("Space Warp Console");
            consoleUIObject.Persist();
            consoleUIObject.transform.SetParent(Chainloader.ManagerObject.transform);
            SpaceWarpConsole con = consoleUIObject.AddComponent<SpaceWarpConsole>();
            consoleUIObject.SetActive(true);

            resolve();
        }
        catch (Exception e)
        {
            SpaceWarpManager.Logger.LogError(e.ToString());
            reject(null);
        }
    }
}
