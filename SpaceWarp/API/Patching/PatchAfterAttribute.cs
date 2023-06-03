using System;

namespace SpaceWarp.API.Patching;

[AttributeUsage(AttributeTargets.Method,AllowMultiple = true)]
public class PatchAfterAttribute : Attribute
{
    public string Guid;

    public PatchAfterAttribute(string guid)
    {
        Guid = guid;
    }
}