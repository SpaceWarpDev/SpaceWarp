using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

/// <summary>
/// An empty config file that does not save anything.
/// </summary>
[PublicAPI]
public class EmptyConfigFile : IConfigFile
{
    /// <inheritdoc />
    public void Save()
    {
    }

    /// <inheritdoc />
    public IConfigEntry this[string section, string key] => throw new KeyNotFoundException($"{section}/{key}");

    /// <inheritdoc />
    public IConfigEntry Bind<T>(string section, string key, T defaultValue = default, string description = "")
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Binds a config entry to a section and key.
    /// </summary>
    /// <param name="section">Section to bind to.</param>
    /// <param name="key">Key to bind to.</param>
    /// <param name="defaultValue">Default value to use if no value is found.</param>
    /// <param name="description">Description of the config entry.</param>
    /// <param name="valueConstraint">Constraint to use for the value.</param>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <returns>The config entry.</returns>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public IConfigEntry Bind<T>(
        string section,
        string key,
        T defaultValue,
        string description,
        IValueConstraint valueConstraint
    )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Sections => new List<string>();

    /// <inheritdoc />
    public IReadOnlyList<string> this[string section] => throw new KeyNotFoundException($"{section}");
}