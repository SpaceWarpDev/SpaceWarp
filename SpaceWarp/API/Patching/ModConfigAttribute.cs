using System;

namespace SpaceWarp.API.Patching;

[AttributeUsage(AttributeTargets.Parameter)]
public class ModConfigAttribute : Attribute
{
    public string Guid;
    public string Section;
    public string Key;

    public ModConfigAttribute(string guid, string section, string key)
    {
        Guid = guid;
        Section = section;
        Key = key;
    }
}