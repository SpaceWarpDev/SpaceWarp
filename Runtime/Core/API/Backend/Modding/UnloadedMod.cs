using System;
using ReduxLib.Configuration;
using SpaceWarp.API.Mods;
using UnityEngine;
using ILogger = ReduxLib.Logging.ILogger;

namespace SpaceWarp.API.Backend.Modding;
internal class UnloadedMod : ISpaceWarpMod
{
    public Type ToLoad;


    public UnloadedMod(Type toLoad)
    {
        ToLoad = toLoad;
    }

    public ISpaceWarpMod Load()
    {
        if (ToLoad.IsSubclassOf(typeof(MonoBehaviour)))
        {
            var modObject = ReduxLib.ReduxLib.GetAlwaysLoadedObject(SWMetadata.Guid);
            modObject.SetActive(false);
            var mb = modObject.AddComponent(ToLoad);
            var mod = (ISpaceWarpMod)mb;
            mod.SWLogger = SWLogger;
            mod.SWConfiguration = SWConfiguration;
            mod.SWMetadata = SWMetadata;
            SWMetadata.Plugin = mod;
            modObject.SetActive(true);
            return mod;
        }
        else
        {
            var mod = (ISpaceWarpMod)Activator.CreateInstance(ToLoad);
            mod.SWLogger = SWLogger;
            mod.SWConfiguration = SWConfiguration;
            mod.SWMetadata = SWMetadata;
            SWMetadata.Plugin = mod;
            return mod;
        }
    }
    
    public void OnPreInitialized() { }

    public void OnInitialized() { }

    public void OnPostInitialized() { }

    public ILogger SWLogger { get; set; }
    public IConfigFile SWConfiguration { get; set; }
    public SpaceWarpPluginDescriptor SWMetadata { get; set; }
}