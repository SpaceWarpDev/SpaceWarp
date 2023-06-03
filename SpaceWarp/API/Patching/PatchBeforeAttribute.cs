using System;

namespace SpaceWarp.API.Patching;

[AttributeUsage(AttributeTargets.Method,AllowMultiple = true)]
public class PatchBeforeAttribute : Attribute
{
    public string Guid;

    public PatchBeforeAttribute(string guid)
    {
        Guid = guid;
    }
}