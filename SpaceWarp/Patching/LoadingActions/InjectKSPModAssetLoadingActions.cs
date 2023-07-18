using System;
using KSP.Game;
using KSP.Game.Flow;
using SpaceWarp.API.Loading;

namespace SpaceWarp.Patching.LoadingActions;

internal class InjectKspModAssetLoadingActions : FlowAction
{
    public InjectKspModAssetLoadingActions() : base("Injecting loading actions for KSP mods")
    {
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        int idx = GameManager.Instance.LoadingFlow._flowIndex + 1;
        foreach (var mod in SpaceWarpManager.InternalModLoaderMods)
        {
            GameManager.Instance.LoadingFlow._flowActions.Insert(idx, new LoadLocalizationAction(mod));
            idx += 1;
            foreach (var action in Loading.FallbackDescriptorLoadingActionGenerators)
            {
                GameManager.Instance.LoadingFlow._flowActions.Insert(idx,action(mod));
                idx += 1;
            }
            foreach (var action in Loading.DescriptorLoadingActionGenerators)
            {
                GameManager.Instance.LoadingFlow._flowActions.Insert(idx,action(mod));
                idx += 1;
            }
        }

        resolve();
    }
}