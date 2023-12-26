using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

/// <summary>
/// A config entry that is stored in a JSON file.
/// </summary>
[PublicAPI]
public class JsonConfigEntry : IConfigEntry
{
    private readonly JsonConfigFile _configFile;
    private object _value;

    /// <summary>
    /// The callbacks that are invoked when the value of this entry changes.
    /// </summary>
    public event Action<object, object> Callbacks;

    /// <summary>
    /// Creates a new config entry.
    /// </summary>
    /// <param name="configFile">Config file that this entry belongs to.</param>
    /// <param name="type">Type of the value.</param>
    /// <param name="description">Description of the value.</param>
    /// <param name="value">Value of the entry.</param>
    /// <param name="constraint">Constraint of the value.</param>
    public JsonConfigEntry(
        JsonConfigFile configFile,
        Type type,
        string description,
        object value,
        IValueConstraint constraint = null
    )
    {
        _configFile = configFile;
        _value = value;
        Constraint = constraint;
        Description = description;
        ValueType = type;
    }

    /// <inheritdoc />
    public object Value
    {
        get => _value;
        set
        {
            Callbacks?.Invoke(_value, value);
            _value = value;
            _configFile.Save();
        }
    }

    /// <inheritdoc />
    public Type ValueType { get; }

    /// <inheritdoc />
    public T Get<T>() where T : class
    {
        if (!typeof(T).IsAssignableFrom(ValueType))
        {
            throw new InvalidCastException($"Cannot cast {ValueType} to {typeof(T)}");
        }

        return Value as T;
    }

    /// <inheritdoc />
    public void Set<T>(T value)
    {
        if (!ValueType.IsAssignableFrom(typeof(T)))
        {
            throw new InvalidCastException($"Cannot cast {ValueType} to {typeof(T)}");
        }
        if (Constraint != null)
        {
            if (!Constraint.IsConstrained(value)) return;
        }
        Value = Convert.ChangeType(value, ValueType);
    }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public IValueConstraint Constraint { get; }

    /// <inheritdoc />
    public void RegisterCallback(Action<object, object> valueChangedCallback)
    {
        Callbacks += valueChangedCallback;
    }
}