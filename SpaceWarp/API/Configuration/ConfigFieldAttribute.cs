using System;

namespace SpaceWarp.API.Configuration;

public class ConfigFieldAttribute : Attribute
{
    public readonly string Name;

    public ConfigFieldAttribute(string name)
    {
        Name = name;
    }
}