using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;

namespace SpaceWarp.Modules;

public abstract class SpaceWarpModule
{
    public ILogger ModuleLogger;
    public IConfigFile ModuleConfiguration;
    public abstract string Name { get; }

    public abstract void LoadModule();
    
    public abstract void PreInitializeModule();

    public abstract void InitializeModule();

    public abstract void PostInitializeModule();
}