using System;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using ReduxLib.Configuration;

namespace SpaceWarp2.API.Config;

/// <summary>
/// Fluent builder for a config entry. Accumulates the entry's shape and binds it at <see cref="Bind" />,
/// returning a live <see cref="ConfigHandle" />.
/// </summary>
[MoonSharpUserData]
public sealed class ConfigBuilder
{
    private readonly IConfigFile _file;
    private readonly string _section;
    private readonly string _name;

    private IConfigType _type;
    private DynValue _default = DynValue.Nil;
    private string _description = string.Empty;
    private string _nameLoc;
    private string _descLoc;
    private const int DefaultSliderSteps = 1024;

    private double _rangeMin;
    private double _rangeMax;
    private int _rangeSteps = DefaultSliderSteps;
    private string _rangeFormat;
    private bool _hasRange;
    private DynValue _values = DynValue.Nil;
    private readonly List<string> _tags = new();

    internal ConfigBuilder(IConfigFile file, string section, string name)
    {
        _file = file;
        _section = section;
        _name = name;
    }

    /// <summary>Sets the entry's config type (built-in such as <c>Double</c>, or a custom type).</summary>
    public ConfigBuilder Type(DynValue type)
    {
        if (type.Type != DataType.UserData || type.UserData.Object is not IConfigType configType)
        {
            throw new ScriptRuntimeException("Config :Type expects a config type such as String, Double, or Color.");
        }

        _type = configType;
        return this;
    }

    /// <summary>Sets the default value, in the type's rich domain. Applied only on the first launch.</summary>
    public ConfigBuilder Default(DynValue value)
    {
        _default = value;
        return this;
    }

    /// <summary>
    /// Constrains an ordered value to an inclusive range. <paramref name="steps" /> sets the slider granularity
    /// and <paramref name="format" /> the slider's number format, which defaults to a type-appropriate format
    /// when omitted.
    /// </summary>
    public ConfigBuilder Range(double min, double max, int steps = DefaultSliderSteps, string format = null)
    {
        _rangeMin = min;
        _rangeMax = max;
        _rangeSteps = steps;
        _rangeFormat = format;
        _hasRange = true;
        return this;
    }

    /// <summary>Constrains the value to an enumerated set, given as a Lua array of rich values.</summary>
    public ConfigBuilder Values(DynValue list)
    {
        _values = list;
        return this;
    }

    /// <summary>Sets the entry's description text.</summary>
    public ConfigBuilder Desc(string text)
    {
        _description = text;
        return this;
    }

    /// <summary>Sets the localization key for the entry's display name in the settings menu.</summary>
    public ConfigBuilder NameLoc(string key)
    {
        _nameLoc = key;
        return this;
    }

    /// <summary>Sets the localization key for the entry's description in the settings menu.</summary>
    public ConfigBuilder DescLoc(string key)
    {
        _descLoc = key;
        return this;
    }

    /// <summary>Attaches a metadata tag to the entry. Multiple allowed.</summary>
    public ConfigBuilder Tag(string name)
    {
        _tags.Add(name);
        return this;
    }

    /// <summary>Binds the entry and returns a live handle over it.</summary>
    public ConfigHandle Bind()
    {
        if (_type == null)
        {
            throw new ScriptRuntimeException($"Config entry '{_section}/{_name}' needs a :Type before :Bind.");
        }

        var storageDefault = _type.Serialize(_default);
        var constraint = BuildConstraint();
        var entry = _file
            .GetOrCreateSection(_section)
            .BindEntry(_type.StorageType, _name, storageDefault, _description, constraint, _nameLoc, _descLoc, _tags);
        return new ConfigHandle(entry, _type);
    }

    private IValueConstraint BuildConstraint()
    {
        if (_hasRange)
        {
            return MakeRange();
        }

        if (_values.Type == DataType.Table)
        {
            return MakeList();
        }

        return null;
    }

    private IValueConstraint MakeRange()
    {
        var storageType = _type.StorageType;
        try
        {
            var constraintType = typeof(RangeConstraint<>).MakeGenericType(storageType);
            var min = Convert.ChangeType(_rangeMin, storageType);
            var max = Convert.ChangeType(_rangeMax, storageType);
            var format = _rangeFormat ?? DefaultRangeFormat(storageType);
            return (IValueConstraint)Activator.CreateInstance(constraintType, min, max, _rangeSteps, format);
        }
        catch (Exception)
        {
            throw new ScriptRuntimeException($"Config :Range is not valid for type '{storageType.Name}'.");
        }
    }

    // Integer types read better without a decimal point. Everything else gets two-decimal float formatting.
    private static string DefaultRangeFormat(Type storageType) => System.Type.GetTypeCode(storageType) switch
    {
        TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16
            or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 => "{0}",
        _ => "{0:F2}"
    };

    private IValueConstraint MakeList()
    {
        var storageType = _type.StorageType;
        try
        {
            var listType = typeof(List<>).MakeGenericType(storageType);
            var list = (IList)Activator.CreateInstance(listType);
            var table = _values.Table;
            for (var i = 1; i <= table.Length; i++)
            {
                list.Add(_type.Serialize(table.Get(i)));
            }

            var constraintType = typeof(ListConstraint<>).MakeGenericType(storageType);
            return (IValueConstraint)Activator.CreateInstance(constraintType, new object[] { list });
        }
        catch (Exception)
        {
            throw new ScriptRuntimeException($"Config :Values is not valid for type '{storageType.Name}'.");
        }
    }
}
