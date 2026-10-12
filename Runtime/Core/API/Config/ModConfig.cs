using System;
using MoonSharp.Interpreter;
using ReduxLib.Configuration;
using SpaceWarp2.API.Mods;

namespace SpaceWarp2.API.Config;

/// <summary>
/// The Lua <c>Config</c> global - a mod's own config namespace. Declare entries through <see cref="Define" />,
/// fetch existing ones through <see cref="Get" />, and reach another mod's config through <see cref="Mod" />.
/// </summary>
/// <remarks>
/// Contributed per mod environment, wrapping that mod's config file.
/// </remarks>
[MoonSharpUserData]
public sealed class ModConfig
{
    private readonly IConfigFile _file;
    private readonly Func<SpaceWarpPluginDescriptor, IConfigFile?> _configOf;

    static ModConfig()
    {
        UserData.RegisterType<ModConfig>();
        UserData.RegisterType<ConfigBuilder>();
        UserData.RegisterType<ConfigHandle>();
        UserData.RegisterType<ConfigView>();
    }

    /// <summary>
    /// Creates a config namespace wrapping the given config file.
    /// </summary>
    /// <param name="file">The mod's config file, or null if the mod has none.</param>
    public ModConfig(IConfigFile file) : this(file, descriptor => descriptor.ConfigFile)
    {
    }

    /// <summary>
    /// Creates a config namespace wrapping the given config file, which reaches other mods' configs of the same
    /// kind, such as their campaign configs.
    /// </summary>
    /// <param name="file">The mod's config file, or null if the mod has none.</param>
    /// <param name="configOf">Picks the config of the same kind from another mod.</param>
    public ModConfig(IConfigFile file, Func<SpaceWarpPluginDescriptor, IConfigFile?> configOf)
    {
        _file = file;
        _configOf = configOf;
    }

    /// <summary>
    /// Begins defining a config entry in the given section and name. Chain the builder methods and finish with
    /// <c>:Bind()</c> to get a live handle.
    /// </summary>
    /// <param name="section">The config section.</param>
    /// <param name="name">The entry name within the section.</param>
    /// <returns>A fluent builder for the entry.</returns>
    public ConfigBuilder Define(string section, string name)
    {
        if (_file == null)
        {
            throw new ScriptRuntimeException("This mod has no config file, so Config:Define is unavailable.");
        }

        return new ConfigBuilder(_file, section, name);
    }

    /// <summary>
    /// Returns a live handle over an existing entry in this mod's own config, or <c>nil</c> if it does not
    /// exist. Use <see cref="Define" /> to create entries.
    /// </summary>
    /// <param name="section">The config section.</param>
    /// <param name="name">The entry name within the section.</param>
    /// <returns>A handle over the entry, or null.</returns>
    public ConfigHandle Get(string section, string name) =>
        _file == null ? null : ConfigView.GetFrom(_file, section, name);

    /// <summary>
    /// Returns a read/write view over another mod's config, reached by its mod id.
    /// </summary>
    /// <param name="modId">The other mod's id.</param>
    /// <returns>A view over that mod's config.</returns>
    /// <exception cref="ScriptRuntimeException">If no mod with that id has a config file.</exception>
    public ConfigView Mod(string modId)
    {
        var descriptor = PluginList.TryGetDescriptor(modId);
        IConfigFile? config = descriptor == null ? null : _configOf(descriptor);
        if (config == null)
        {
            throw new ScriptRuntimeException($"No config found for mod '{modId}'.");
        }

        return new ConfigView(config);
    }
}
