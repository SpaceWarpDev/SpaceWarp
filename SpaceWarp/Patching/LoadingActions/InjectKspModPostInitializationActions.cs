using System;
using KSP.Game;
using KSP.Game.Flow;

namespace SpaceWarp.Patching.LoadingActions;

internal class InjectKspModPostInitializationActions : FlowAction
{
    public InjectKspModPostInitializationActions() : base("Injecting loading actions for KSP mods")
    {
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        foreach (var mod in SpaceWarpManager.InternalModLoaderMods)
        {
            GameManager.Instance.LoadingFlow.AddAction(new InitializeModAction(mod));;
        }

        resolve();
    }
}