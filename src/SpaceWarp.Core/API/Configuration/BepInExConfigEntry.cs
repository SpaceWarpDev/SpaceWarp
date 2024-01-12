using BepInEx.Configuration;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

/// <summary>
/// A wrapper around a BepInEx <see cref="ConfigEntryBase"/> to make it compatible with the <see cref="IConfigEntry"/>
/// interface.
/// </summary>
[PublicAPI]
public class BepInExConfigEntry :  IConfigEntry
{
    /// <summary>
    /// The underlying <see cref="ConfigEntryBase"/> that this class wraps.
    /// </summary>
    public readonly ConfigEntryBase EntryBase;

    /// <summary>
    /// The callbacks that are invoked when the value of this entry changes.
    /// </summary>
    public event Action<object, object> Callbacks;

    /// <summary>
    /// Creates a new <see cref="BepInExConfigEntry"/> from the given <see cref="ConfigEntryBase"/> and optional
    /// <see cref="IValueConstraint"/>.
    /// </summary>
    /// <param name="entryBase">The <see cref="ConfigEntryBase"/> to wrap.</param>
    /// <param name="constraint">The <see cref="IValueConstraint"/> to use.</param>
    public BepInExConfigEntry(ConfigEntryBase entryBase, IValueConstraint constraint = null)
    {
        EntryBase = entryBase;
        Constraint = constraint;
    }

    /// <inheritdoc />
    public object Value
    {
        get => EntryBase.BoxedValue;
        set
        {
            Callbacks?.Invoke(EntryBase.BoxedValue, value);
            EntryBase.BoxedValue = value;
        }
    }

    /// <inheritdoc />
    public Type ValueType => EntryBase.SettingType;

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
        EntryBase.BoxedValue = Convert.ChangeType(value, ValueType);
    }

    /// <inheritdoc />
    public string Description => EntryBase.Description.Description;

    /// <inheritdoc />
    public IValueConstraint Constraint { get; }

    /// <inheritdoc />
    public void RegisterCallback(Action<object, object> valueChangedCallback)
    {
        Callbacks += valueChangedCallback;
    }
}