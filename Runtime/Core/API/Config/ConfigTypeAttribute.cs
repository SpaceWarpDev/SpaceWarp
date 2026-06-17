using System;
using JetBrains.Annotations;

namespace SpaceWarp2.API.Config;

/// <summary>
/// Marks an <see cref="IConfigType" /> implementation as a config type and names the Lua global it is exposed
/// under. Discovered across every loaded assembly, so a mod ships its own config types by tagging them.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[PublicAPI]
public sealed class ConfigTypeAttribute : Attribute
{
    /// <summary>
    /// The Lua global the config type is exposed under (for example <c>Color</c>).
    /// </summary>
    public string GlobalName { get; }

    /// <summary>
    /// Marks a config type and names the Lua global it is exposed under.
    /// </summary>
    /// <param name="globalName">The Lua global the config type is exposed under.</param>
    public ConfigTypeAttribute(string globalName)
    {
        GlobalName = globalName;
    }
}
