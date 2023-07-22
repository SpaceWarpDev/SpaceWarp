using SpaceWarp.API.Configuration;

namespace SpaceWarp.Modules;

public class VersionChecking : SpaceWarpModule
{
    public override string Name { get; }
    public ConfigValue<bool> ConfigFirstLaunch;
    public ConfigValue<bool> ConfigCheckVersions;
    public override void LoadModule()
    {
        
    }

    public override void PreInitializeModule()
    {
        throw new System.NotImplementedException();
    }

    public override void InitializeModule()
    {
        
        if (ConfigCheckVersions.Value)
        {
            CheckVersions();
        }
    }

    public override void PostInitializeModule()
    {
        throw new System.NotImplementedException();
    }
    
    public void ClearVersions()
    {
        foreach (var plugin in SpaceWarpManager.AllPlugins)
        {
            SpaceWarpManager.ModsOutdated[plugin.Guid] = false;
        }
    }
    public void CheckVersions()
    {
        ClearVersions();
        foreach (var plugin in SpaceWarpManager.AllPlugins)
        {
            if (plugin.SWInfo.VersionCheck != null)
            {
                StartCoroutine(CheckVersion(plugin.Guid, plugin.SWInfo));
            }
        }

        foreach (var info in SpaceWarpManager.DisabledPlugins)
        {
            if (info.SWInfo.VersionCheck != null)
            {
                StartCoroutine(CheckVersion(info.Guid, info.SWInfo));
            }
        }
    }
}