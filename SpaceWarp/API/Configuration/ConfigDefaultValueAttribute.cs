using System;

namespace SpaceWarp.API.Configuration;

public class ConfigDefaultValueAttribute : Attribute
{
    public readonly object DefaultValue;

    public ConfigDefaultValueAttribute(object defaultValue)
    {
        DefaultValue = defaultValue;
    }
}