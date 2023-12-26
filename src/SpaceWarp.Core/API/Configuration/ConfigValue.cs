using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

/// <summary>
/// A wrapper around <see cref="IConfigEntry"/> that provides type safety.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
[PublicAPI]
public class ConfigValue<T>
{
    /// <summary>
    /// The underlying <see cref="IConfigEntry"/>.
    /// </summary>
    public IConfigEntry Entry;

    /// <summary>
    /// Creates a new <see cref="ConfigValue{T}"/> from an <see cref="IConfigEntry"/>.
    /// </summary>
    /// <param name="entry">The entry to wrap.</param>
    /// <exception cref="ArgumentException">
    /// If the type of <paramref name="entry"/> does not match <typeparamref name="T"/>.
    /// </exception>
    public ConfigValue(IConfigEntry entry)
    {
        Entry = entry;
        if (typeof(T) != entry.ValueType)
        {
            throw new ArgumentException(nameof(entry));
        }
    }

    /// <summary>
    /// The value of the entry.
    /// </summary>
    public T Value
    {
        get => (T)Entry.Value;
        set => Entry.Value = value;
    }

    /// <summary>
    /// Registers a callback that will be invoked when the value changes.
    /// </summary>
    /// <param name="callback">The callback to invoke.</param>
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