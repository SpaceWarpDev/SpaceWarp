using System;
using System.IO;
using KSP.Game;
using KSP.Game.Flow;
using KSP.Game.Load;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

public class LoadCampaignConfiguration : FlowAction
{
    public LoadCampaignConfiguration() : base("Loading per campaign mod configuration")
    {
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        var path =
            $"{GameManager.Instance.Game.SaveLoadManager.ActiveCampaignFolderPath}{Path.DirectorySeparatorChar}mod_data";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        foreach (var mod in PluginList.AllEnabledAndActivePlugins)
        {
            if (mod.SaveConfigFile == null || mod.SaveConfigFile.Sections.Count == 0) continue;
            var newConfigFile = new JsonConfigFile($"{path}{Path.DirectorySeparatorChar}{mod.Guid}.cfg");
            mod.SaveConfigFile.CurrentConfigFile = newConfigFile;
        }

        resolve();
    }
}