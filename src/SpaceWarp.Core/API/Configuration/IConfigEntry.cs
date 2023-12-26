using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

/// <summary>
/// Represents a config entry
/// </summary>
[PublicAPI]
public interface IConfigEntry
{
    /// <summary>
    /// The value of the config entry
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// The type of the value of the config entry
    /// </summary>
    public Type ValueType { get; }

    /// <summary>
    /// Gets the value of the config entry as a specific type
    /// </summary>
    /// <typeparam name="T">The type to cast to</typeparam>
    /// <returns>The value as the specified type</returns>
    public T Get<T>() where T : class;

    /// <summary>
    /// Sets the value of the config entry
    /// </summary>
    /// <param name="value">The value to set</param>
    /// <typeparam name="T">The type of the value</typeparam>
    public void Set<T>(T value);

    /// <summary>
    /// The description of the config entry
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The value constraint of the config entry
    /// </summary>
    public IValueConstraint Constraint { get; }

    /// <summary>
    /// Registers a callback to be called when setting the value on a config file
    /// </summary>
    /// <param name="valueChangedCallback">
    /// An action that takes the old value and the new value and calls a callback
    /// </param>
    public void RegisterCallback(Action<object, object> valueChangedCallback);
}