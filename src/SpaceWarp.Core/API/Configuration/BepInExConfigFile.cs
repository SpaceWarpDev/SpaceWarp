using BepInEx.Configuration;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

/// <summary>
/// A wrapper around BepInEx's <see cref="ConfigFile" /> to make it implement <see cref="IConfigFile" />.
/// </summary>
[PublicAPI]
public class BepInExConfigFile : IConfigFile
{
    /// <summary>
    /// The underlying <see cref="ConfigFile" /> instance.
    /// </summary>
    public readonly ConfigFile AdaptedConfigFile;

    /// <summary>
    /// Creates a new <see cref="BepInExConfigFile" /> instance.
    /// </summary>
    /// <param name="adaptedConfigFile">The <see cref="ConfigFile" /> instance to wrap.</param>
    public BepInExConfigFile(ConfigFile adaptedConfigFile)
    {
        AdaptedConfigFile = adaptedConfigFile;
    }

    /// <inheritdoc />
    public void Save()
    {
        AdaptedConfigFile.Save();
    }

    private Dictionary<(string section, string key), IConfigEntry> _storedEntries = new();

    /// <inheritdoc />
    public IConfigEntry this[string section, string key] => _storedEntries.TryGetValue((section, key), out var result)
        ? result
        : _storedEntries[(section, key)] = new BepInExConfigEntry(AdaptedConfigFile[section, key],
            IValueConstraint.FromAcceptableValueBase(AdaptedConfigFile[section, key].Description.AcceptableValues));

    /// <inheritdoc />
    public IConfigEntry Bind<T>(string section, string key, T defaultValue = default, string description = "")
    {
        return new BepInExConfigEntry(AdaptedConfigFile.Bind(section, key, defaultValue, description));
    }

    /// <summary>
    /// Binds a new config entry to the given section and key with the given default value and description.
    /// </summary>
    /// <param name="section">Section to bind the entry to.</param>
    /// <param name="key">Key to bind the entry to.</param>
    /// <param name="defaultValue">Default value of the entry.</param>
    /// <param name="description">Description of the entry.</param>
    /// <param name="valueConstraint">Value constraint of the entry.</param>
    /// <typeparam name="T">Type of the entry.</typeparam>
    /// <returns>The newly bound config entry.</returns>
    public IConfigEntry Bind<T>(
        string section,
        string key,
        T defaultValue,
        string description,
        IValueConstraint valueConstraint
    )
    {
        return new BepInExConfigEntry(AdaptedConfigFile.Bind(new ConfigDefinition(section, key), defaultValue,
            new ConfigDescription(description, valueConstraint.ToAcceptableValueBase())));
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Sections => AdaptedConfigFile.Keys.Select(x => x.Section).Distinct().ToList();

    /// <inheritdoc />
    public IReadOnlyList<string> this[string section] => AdaptedConfigFile.Keys.Where(x => x.Section == section)
        .Select(x => x.Key).ToList();
}