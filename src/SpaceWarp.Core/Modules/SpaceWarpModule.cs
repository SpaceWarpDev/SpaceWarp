using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;

namespace SpaceWarp.Modules;

/// <summary>
/// Base class for SpaceWarp modules.
/// </summary>
public abstract class SpaceWarpModule
{
    /// <summary>
    /// The logger for the module.
    /// </summary>
    public ILogger ModuleLogger;

    /// <summary>
    /// The configuration file for the module.
    /// </summary>
    public IConfigFile ModuleConfiguration;

    /// <summary>
    /// The name of the module.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Loads the module. Called in plugin's Awake method.
    /// </summary>
    public virtual void LoadModule() {}

    /// <summary>
    /// 1st stage of module initialization. Called in plugin's OnPreInitialized method.
    /// </summary>
    public virtual void PreInitializeModule() {}

    /// <summary>
    /// 2nd stage of module initialization. Called in plugin's OnInitialized method.
    /// </summary>
    public virtual void InitializeModule() {}

    /// <summary>
    /// 3rd stage of module initialization. Called in plugin's OnPostInitialized method.
    /// </summary>
    public virtual void PostInitializeModule() {}

    /// <summary>
    /// Names of modules that this module depends on.
    /// </summary>
    public virtual List<string> Prerequisites => new();
}