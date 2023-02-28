using System;
using KSP.Game.Flow;
using SpaceWarp.API;
using SpaceWarp.API.Managers;

namespace SpaceWarp.Patching.LoadingActions;

public class LoadSpaceWarpLocalizationsAction : FlowAction
{
    public LoadSpaceWarpLocalizationsAction(string name) : base(name)
    {
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        ManagerLocator.TryGet(out SpaceWarpManager spaceWarpManager);

        try
        {
            spaceWarpManager.LoadSpaceWarpLocalizations();
            resolve();
        }
        catch (Exception e)
        {
            reject(e.ToString());
        }
    }
}