using System;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

[PublicAPI]
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
        set
        {
            Entry.Value = value;
        }
    }

    public void RegisterCallback(Action<T, T> callback)
    {
        // Callbacks += callback;
        Entry.RegisterCallback(NewCallback);
        return;

        void NewCallback(object from, object to)
        {
            callback.Invoke((T)from, (T)to);
        }
    }
}