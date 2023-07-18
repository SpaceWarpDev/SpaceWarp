using System;
using KSP.Game;
using KSP.Game.Flow;

namespace SpaceWarp.Patching.LoadingActions;

internal class InjectKspModInitializationActions : FlowAction
{
    public InjectKspModInitializationActions() : base("Injecting loading actions for KSP mods")
    {
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        int idx = GameManager.Instance.LoadingFlow._flowIndex + 1;
        foreach (var mod in SpaceWarpManager.InternalModLoaderMods)
        {
            GameManager.Instance.LoadingFlow._flowActions.Insert(idx, new InitializeModAction(mod));
            idx += 1;
        }

        resolve();
    }
}