using System;
using ReduxLib.Configuration;
using SpaceWarp2.API.Mods;
using UnityEngine;
using ILogger = ReduxLib.Logging.ILogger;

namespace SpaceWarp2.API.Backend.Modding;
internal class UnloadedMod : ISpaceWarpMod
{
    public readonly Type ToLoad;

    public UnloadedMod(Type toLoad)
    {
        ToLoad = toLoad;
    }

    public ISpaceWarpMod Load()
    {
        GameObject modObject = null;
        ISpaceWarpMod mod;
        if (ToLoad.IsSubclassOf(typeof(MonoBehaviour)))
        {
            modObject = ReduxLib.ReduxLib.GetAlwaysLoadedObject(SWMetadata.Guid);
            modObject.SetActive(false);
            mod = (ISpaceWarpMod)modObject.AddComponent(ToLoad);
        }
        else
        {
            mod = (ISpaceWarpMod)Activator.CreateInstance(ToLoad);
        }

        mod.SWLogger = SWMetadata.Logger;
        mod.SWConfiguration = SWConfiguration;
        mod.SWMetadata = SWMetadata;
        SWMetadata.Plugin = mod;
        modObject?.SetActive(true);
        return mod;
    }
    
    public void OnPreInitialized() { }

    public void OnInitialized() { }

    public void OnPostInitialized() { }

    public ILogger SWLogger { get; set; }
    public IConfigFile SWConfiguration { get; set; }
    public SpaceWarpPluginDescriptor SWMetadata { get; set; }
}