using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

/// <summary>
/// Represents a configuration file.
/// </summary>
[PublicAPI]
public interface IConfigFile
{
    /// <summary>
    /// Saves the configuration file.
    /// </summary>
    public void Save();

    /// <summary>
    /// Gets the <see cref="IConfigEntry" /> with the specified section and key.
    /// </summary>
    /// <param name="section">Section of the entry.</param>
    /// <param name="key">Key of the entry.</param>
    public IConfigEntry this[string section, string key] { get; }

    /// <summary>
    /// Binds a new <see cref="IConfigEntry" /> to the specified section and key.
    /// </summary>
    /// <param name="section">Section of the entry.</param>
    /// <param name="key">Key of the entry.</param>
    /// <param name="defaultValue">Default value of the entry.</param>
    /// <param name="description">Description of the entry.</param>
    /// <typeparam name="T">Type of the entry.</typeparam>
    /// <returns>The bound <see cref="IConfigEntry" />.</returns>
    public IConfigEntry Bind<T>(string section, string key, T defaultValue = default, string description = "");

    /// <summary>
    /// A list of all sections in the configuration file.
    /// </summary>
    public IReadOnlyList<string> Sections { get; }

    /// <summary>
    /// A list of all keys in the specified section.
    /// </summary>
    /// <param name="section"></param>
    public IReadOnlyList<string> this[string section] { get; }
}