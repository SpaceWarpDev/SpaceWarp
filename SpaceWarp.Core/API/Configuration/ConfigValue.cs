using System;

namespace SpaceWarp.API.Configuration;

public class ConfigValue<T>
{
    public IConfigEntry Entry;

    public ConfigValue(IConfigEntry entry)
    {
        Entry = entry;
        if (typeof(T) != entry.ValueType)
        {
            throw new ArgumentException(nameof(entry));
        }
    }

    public T Value
    {
        get => (T)Entry.Value;
        set => Entry.Value = value;
    }
}