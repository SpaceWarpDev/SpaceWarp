using System;
using BepInEx.Configuration;

namespace SpaceWarp.API.Configuration;

public class BepInExConfigEntry :  IConfigEntry
{
    public readonly ConfigEntryBase EntryBase;

    public BepInExConfigEntry(ConfigEntryBase entryBase)
    {
        EntryBase = entryBase;
    }

    public object Value
    {
        get => EntryBase.BoxedValue;
        set => EntryBase.BoxedValue = value;
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

        EntryBase.BoxedValue = Convert.ChangeType(value, ValueType);
    }

    public string Description => EntryBase.Description.Description;
}