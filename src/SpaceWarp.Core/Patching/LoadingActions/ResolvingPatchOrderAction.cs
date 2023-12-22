using System;
using KSP.Game.Flow;

namespace SpaceWarp.Patching.LoadingActions;

internal class ResolvingPatchOrderAction : FlowAction
{
    public ResolvingPatchOrderAction(string name) : base(name)
    {
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        throw new NotImplementedException();
    }
}