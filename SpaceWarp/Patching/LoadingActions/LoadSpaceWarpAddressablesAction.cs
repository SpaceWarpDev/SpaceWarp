using System;
using System.IO;
using KSP.Game.Flow;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class LoadSpaceWarpAddressablesAction : FlowAction
{
    public LoadSpaceWarpAddressablesAction() : base("Initializing Space Warp Provided Addressables")
    {
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            string addressablesPath = Path.Combine(SpaceWarpManager.SpaceWarpFolder, "addressables");
            string catalogPath = Path.Combine(addressablesPath, "catalog.json");
            if (File.Exists(catalogPath))
            {
                AssetHelpers.LoadAddressable(catalogPath);
            }
            resolve();
        }
        catch (Exception e)
        {
            SpaceWarpManager.Logger.LogError(e.ToString());
            reject(null);
        }
    }
}
