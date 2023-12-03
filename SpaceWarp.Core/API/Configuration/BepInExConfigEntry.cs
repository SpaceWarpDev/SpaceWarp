using System;
using BepInEx.Configuration;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

[PublicAPI]
public class BepInExConfigEntry :  IConfigEntry
{
    public readonly ConfigEntryBase EntryBase;
    public event Action<object, object> Callbacks; 
    public BepInExConfigEntry(ConfigEntryBase entryBase, IValueConstraint constraint = null)
    {
        EntryBase = entryBase;
        Constraint = constraint;
    }

    public object Value
    {
        get => EntryBase.BoxedValue;
        set
        {
            Callbacks?.Invoke(EntryBase.BoxedValue, value);
            EntryBase.BoxedValue = value;
        }
    }

    public Type ValueType => EntryBase.SettingType;

    public T Get<T>() where T : class
    {
        if (!typeof(T).IsAssignableFrom(ValueType))
        {
            throw new InvalidCastException($"Cannot cast {ValueType} to {typeof(T)}");
        }

        return Value as T;
    }

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
        Callbacks?.Invoke(EntryBase.BoxedValue, value);
        EntryBase.BoxedValue = Convert.ChangeType(value, ValueType);
    }

    public string Description => EntryBase.Description.Description;
    public IValueConstraint Constraint { get; }
    public void RegisterCallback(Action<object, object> valueChangedCallback)
    {
        Callbacks += valueChangedCallback;
    }
}