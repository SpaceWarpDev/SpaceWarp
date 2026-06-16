using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace SpaceWarp2.API.Config;

/// <summary>
/// The built-in config types: seeds them as globals on a mod environment, and resolves a stored value's C#
/// type back to its config type for cross-mod reads.
/// </summary>
public static class ConfigTypes
{
    // Shared singletons - the built-in types are stateless, so one instance each serves every env and the
    // storage-type resolver below.
    private static readonly Dictionary<Type, IConfigType> ByStorageType = new()
    {
        { typeof(string), new StringType() },
        { typeof(int), new IntegerType() },
        { typeof(double), new DoubleType() },
        { typeof(bool), new BooleanType() },
        { typeof(Color), new ColorType() }
    };

    static ConfigTypes()
    {
        UserData.RegisterType<StringType>();
        UserData.RegisterType<IntegerType>();
        UserData.RegisterType<DoubleType>();
        UserData.RegisterType<BooleanType>();
        UserData.RegisterType<ColorType>();
    }

    /// <summary>
    /// Seeds the built-in config type globals (<c>String</c>, <c>Integer</c>, <c>Double</c>, <c>Boolean</c>,
    /// <c>Color</c>) onto a mod environment's globals table.
    /// </summary>
    /// <param name="globals">The mod environment's globals table.</param>
    public static void SeedOn(Table globals)
    {
        globals["String"] = ByStorageType[typeof(string)];
        globals["Integer"] = ByStorageType[typeof(int)];
        globals["Double"] = ByStorageType[typeof(double)];
        globals["Boolean"] = ByStorageType[typeof(bool)];
        globals["Color"] = ByStorageType[typeof(Color)];
    }

    /// <summary>
    /// Resolves a stored value's C# type back to its built-in config type, for reading an entry whose original
    /// Lua type was not recorded (cross-mod and own <c>:Get</c>).
    /// </summary>
    /// <param name="storageType">The entry's stored value type.</param>
    /// <returns>The matching config type.</returns>
    /// <exception cref="ScriptRuntimeException">If no built-in type stores that C# type.</exception>
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
[MoonSharpUserData]
public sealed class StringType : IConfigType
{
    public Type StorageType => typeof(string);

    public object Serialize(DynValue value) => value.CastToString() ?? string.Empty;

    public DynValue Deserialize(object storage) => DynValue.NewString((string)storage);
}

/// <summary>An integer config value, stored as an int.</summary>
[MoonSharpUserData]
public sealed class IntegerType : IConfigType
{
    public Type StorageType => typeof(int);

    public object Serialize(DynValue value) => (int)(value.CastToNumber() ?? 0d);

    public DynValue Deserialize(object storage) => DynValue.NewNumber(Convert.ToInt32(storage));
}

/// <summary>A floating-point config value, stored as a double.</summary>
[MoonSharpUserData]
public sealed class DoubleType : IConfigType
{
    public Type StorageType => typeof(double);

    public object Serialize(DynValue value) => value.CastToNumber() ?? 0d;

    public DynValue Deserialize(object storage) => DynValue.NewNumber(Convert.ToDouble(storage));
}

/// <summary>A boolean config value, stored as a bool.</summary>
[MoonSharpUserData]
public sealed class BooleanType : IConfigType
{
    public Type StorageType => typeof(bool);

    public object Serialize(DynValue value) => value.CastToBool();

    public DynValue Deserialize(object storage) => DynValue.NewBoolean((bool)storage);
}

/// <summary>
/// A color config value: a <see cref="LuaColor" /> on the script side, a <c>UnityEngine.Color</c> in storage
/// so the settings menu renders it as a color picker. Accepts a <see cref="LuaColor" /> or a table (keyed
/// <c>{r=, g=, b=, a=}</c> or indexed <c>{1, 0.5, 0, 1}</c>) when serializing.
/// </summary>
[MoonSharpUserData]
public sealed class ColorType : IConfigType
{
    public Type StorageType => typeof(Color);

    public object Serialize(DynValue value)
    {
        if (value.Type == DataType.UserData && value.UserData.Object is LuaColor luaColor)
        {
            return luaColor.ToColor();
        }

        return TableToColor(value.Table);
    }

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
