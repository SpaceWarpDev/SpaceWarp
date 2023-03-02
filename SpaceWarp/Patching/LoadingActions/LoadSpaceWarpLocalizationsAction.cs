using System;
using System.IO;
using KSP.Game.Flow;

namespace SpaceWarp.Patching.LoadingActions;

public class LoadSpaceWarpLocalizationsAction : FlowAction
{
    public LoadSpaceWarpLocalizationsAction() : base("Loading Space Warp localizations")
    {
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            if (I2.Loc.LocalizationManager.Sources.Count == 0)
            {
                I2.Loc.LocalizationManager.UpdateSources();
            }

            string localizationsPath = Path.Combine(SpaceWarpManager.SpaceWarpFolder, "localizations");
            AssetHelpers.LoadLocalizationFromFolder(localizationsPath);
            resolve();
        }
        catch (Exception e)
        {
            SpaceWarpManager.Logger.LogError(e.ToString());
            reject(null);
        }
    }
}