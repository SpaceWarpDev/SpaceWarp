﻿using System;
using KSP.Game.Flow;
using SpaceWarp.API;
using SpaceWarp.API.Managers;

namespace SpaceWarp.Patching.LoadingActions;

public class AfterModsLoadedAction : FlowAction
{
    public AfterModsLoadedAction(string name) : base(name)
    {
        //
    }

    protected override void DoAction(Action resolve, Action<string> reject)
    {
        ManagerLocator.TryGet(out SpaceWarpManager spaceWarpManager);

        try
        {
            spaceWarpManager.InvokePostInitializeModsAfterAllModsLoaded();
            resolve();
        }
        catch (Exception e)
        {
            reject(e.ToString());
        }
    }
}