using ReduxLib.Configuration;
using UnityEngine;
using ILogger = ReduxLib.Logging.ILogger;

namespace SpaceWarp.API.Mods;

public abstract class MonoBehaviourMod : MonoBehaviour, ISpaceWarpMod
{
    public virtual void OnPreInitialized()
    {
        
    }

    public virtual void OnInitialized()
    {
        
    }

    public virtual void OnPostInitialized()
    {
        
    }


    public ILogger SWLogger { get; set;  }
    public IConfigFile SWConfiguration { get; set; }
    public SpaceWarpPluginDescriptor SWMetadata { get; set; }
}