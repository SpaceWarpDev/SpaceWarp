using MoonSharp.Interpreter;
using ReduxLib.Configuration;

namespace SpaceWarp2.API.Config;

/// <summary>
/// A live view over a bound config entry. Reads and writes go through the entry's config type, so scripts see
/// rich values while storage holds the C# value the settings menu renders.
/// </summary>
/// <remarks>
/// The handle is live, not a captured value: the settings menu or another mod can move the underlying entry,
/// and <c>value</c> always reflects the current setting.
/// </remarks>
[MoonSharpUserData]
public sealed class ConfigHandle
{
    private readonly IConfigEntry _entry;
    private readonly IConfigType _type;

    internal ConfigHandle(IConfigEntry entry, IConfigType type)
    {
        _entry = entry;
        _type = type;
    }

    /// <summary>
    /// The current value in the config type's rich Lua domain. Reading deserializes the stored value. Writing
    /// serializes, stores (which saves the file), and fires change callbacks.
    /// </summary>
    public DynValue value
    {
        get => _type.Deserialize(_entry.Value);
        set => _entry.Value = _type.Serialize(value);
    }

    /// <summary>
    /// Registers a callback fired whenever the value changes, receiving the old and new values in the rich
    /// Lua domain.
    /// </summary>
    /// <param name="fn">The Lua callback, called as <c>fn(old, new)</c>.</param>
    public void OnChange(DynValue fn)
    {
        if (fn.Type != DataType.Function)
        {
            throw new ScriptRuntimeException("Config :OnChange expects a function.");
        }

        var closure = fn.Function;
        _entry.RegisterCallback((oldValue, newValue) =>
            closure.Call(_type.Deserialize(oldValue), _type.Deserialize(newValue)));
    }

    /// <summary>Determines whether the entry carries the given metadata tag.</summary>
    /// <param name="tag">The tag to check for.</param>
    /// <returns>True if the entry has the tag, false otherwise.</returns>
    public bool HasTag(string tag) => _entry.HasTag(tag);
}
