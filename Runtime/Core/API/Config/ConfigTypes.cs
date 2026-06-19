using System;
using System.Collections.Generic;
using System.Reflection;
using MoonSharp.Interpreter;
using ReduxLib.Reflection;
using UnityEngine;
using ILogger = ReduxLib.Logging.ILogger;

namespace SpaceWarp2.API.Config;

/// <summary>
/// The registry of config types. Discovers every <see cref="IConfigType" /> tagged with
/// <see cref="ConfigTypeAttribute" /> across the loaded assemblies, contributes them as globals on a mod
/// environment, and resolves a stored value's C# type back to its config type for cross-mod reads.
/// </summary>
public static class ConfigTypes
{
    private static readonly ILogger Logger = ReduxLib.ReduxLib.GetLogger("ConfigTypes");
    private static readonly Dictionary<Type, IConfigType> ByStorageType = new();
    private static readonly Dictionary<string, IConfigType> ByGlobalName = new();

    static ConfigTypes()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetLoadableTypes())
            {
                if (type.IsAbstract || type.IsInterface || !typeof(IConfigType).IsAssignableFrom(type)) continue;
                var attribute = type.GetCustomAttribute<ConfigTypeAttribute>();
                if (attribute == null) continue;
                Register(type, attribute.GlobalName);
            }
        }
    }

    private static void Register(Type type, string globalName)
    {
        try
        {
            UserData.RegisterType(type);
            var instance = (IConfigType)Activator.CreateInstance(type);
            if (ByGlobalName.ContainsKey(globalName))
            {
                Logger.LogWarning($"Config type name '{globalName}' is already registered. '{type.FullName}' overrides it.");
            }
            ByStorageType[instance.StorageType] = instance;
            ByGlobalName[globalName] = instance;
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to register config type '{type.FullName}': {e}");
        }
    }

    /// <summary>
    /// Contributes the discovered config type globals (<c>String</c>, <c>Integer</c>, <c>Double</c>,
    /// <c>Boolean</c>, <c>Color</c>, plus any a mod adds) onto a mod environment's globals table.
    /// </summary>
    /// <param name="globals">The mod environment's globals table.</param>
    public static void ContributeTo(Table globals)
    {
        foreach (var pair in ByGlobalName)
        {
            globals[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// Resolves a stored value's C# type back to its config type, for reading an entry whose original Lua type
    /// was not recorded (cross-mod and own <c>:Get</c>).
    /// </summary>
    /// <param name="storageType">The entry's stored value type.</param>
    /// <returns>The matching config type.</returns>
    /// <exception cref="ScriptRuntimeException">If no config type stores that C# type.</exception>
    public static IConfigType Resolve(Type storageType)
    {
        if (ByStorageType.TryGetValue(storageType, out var type))
        {
            return type;
        }

        throw new ScriptRuntimeException($"Config :Get does not support stored type '{storageType.Name}'.");
    }
}

/// <summary>A string config value, stored as a string.</summary>
[ConfigType("String")]
[MoonSharpUserData]
public sealed class StringType : IConfigType
{
    /// <inheritdoc />
    public Type StorageType => typeof(string);

    /// <inheritdoc />
    public object Serialize(DynValue value) => value.CastToString() ?? string.Empty;

    /// <inheritdoc />
    public DynValue Deserialize(object storage) => DynValue.NewString((string)storage);
}

/// <summary>An integer config value, stored as an int.</summary>
[ConfigType("Integer")]
[MoonSharpUserData]
public sealed class IntegerType : IConfigType
{
    /// <inheritdoc />
    public Type StorageType => typeof(int);

    /// <inheritdoc />
    public object Serialize(DynValue value) => (int)(value.CastToNumber() ?? 0d);

    /// <inheritdoc />
    public DynValue Deserialize(object storage) => DynValue.NewNumber(Convert.ToInt32(storage));
}

/// <summary>A floating-point config value, stored as a double.</summary>
[ConfigType("Double")]
[MoonSharpUserData]
public sealed class DoubleType : IConfigType
{
    /// <inheritdoc />
    public Type StorageType => typeof(double);

    /// <inheritdoc />
    public object Serialize(DynValue value) => value.CastToNumber() ?? 0d;

    /// <inheritdoc />
    public DynValue Deserialize(object storage) => DynValue.NewNumber(Convert.ToDouble(storage));
}

/// <summary>A boolean config value, stored as a bool.</summary>
[ConfigType("Boolean")]
[MoonSharpUserData]
public sealed class BooleanType : IConfigType
{
    /// <inheritdoc />
    public Type StorageType => typeof(bool);

    /// <inheritdoc />
    public object Serialize(DynValue value) => value.CastToBool();

    /// <inheritdoc />
    public DynValue Deserialize(object storage) => DynValue.NewBoolean((bool)storage);
}

/// <summary>
/// A color config value: a <see cref="LuaColor" /> on the script side, a <c>UnityEngine.Color</c> in storage
/// so the settings menu renders it as a color picker. Accepts a <see cref="LuaColor" /> or a table (keyed
/// <c>{r=, g=, b=, a=}</c> or indexed <c>{1, 0.5, 0, 1}</c>) when serializing.
/// </summary>
[ConfigType("Color")]
[MoonSharpUserData]
public sealed class ColorType : IConfigType
{
    /// <inheritdoc />
    public Type StorageType => typeof(Color);

    /// <inheritdoc />
    public object Serialize(DynValue value)
    {
        if (value.Type == DataType.UserData && value.UserData.Object is LuaColor luaColor)
        {
            return luaColor.ToColor();
        }

        return TableToColor(value.Table);
    }

    /// <inheritdoc />
    public DynValue Deserialize(object storage) => UserData.Create(LuaColor.FromColor((Color)storage));

    private static Color TableToColor(Table t)
    {
        if (t == null)
        {
            return default;
        }

        return new Color(
            ReadChannel(t, 1, "r", "red", 0f),
            ReadChannel(t, 2, "g", "green", 0f),
            ReadChannel(t, 3, "b", "blue", 0f),
            ReadChannel(t, 4, "a", "alpha", 1f)
        );
    }

    private static float ReadChannel(Table t, int index, string shortKey, string longKey, float fallback)
    {
        var v = t.Get(shortKey);
        if (v.Type == DataType.Number)
        {
            return (float)v.Number;
        }

        v = t.Get(longKey);
        if (v.Type == DataType.Number)
        {
            return (float)v.Number;
        }

        v = t.Get(index);
        if (v.Type == DataType.Number)
        {
            return (float)v.Number;
        }

        return fallback;
    }
}
