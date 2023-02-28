using System;
using KSP.Game.Flow;
using SpaceWarp.API;
using SpaceWarp.API.Managers;

namespace SpaceWarp.Patching.LoadingActions;

public class ReadingModsAction : FlowAction
{
    public ReadingModsAction(string name) : base(name)
    {
        //
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        ManagerLocator.TryGet(out SpaceWarpManager spaceWarpManager);

        try
        {
            spaceWarpManager.ReadMods();
            resolve();
        }
        catch (Exception e)
        {
            reject(e.ToString());
        }
    }
}