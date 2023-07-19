namespace SpaceWarp.Patching.LoadingActions;

using System;
using KSP.Game;
using KSP.Game.Flow;

internal class InjectKspModPreInitializationActions : FlowAction
{
    public InjectKspModPreInitializationActions() : base("Injecting loading actions for KSP mods")
    {
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        int idx = GameManager.Instance.LoadingFlow._flowIndex + 1;
        foreach (var mod in SpaceWarpManager.InternalModLoaderMods)
        {
            GameManager.Instance.LoadingFlow._flowActions.Insert(idx, new PreInitializeModAction(mod));
            idx += 1;
        }

        resolve();
    }
}