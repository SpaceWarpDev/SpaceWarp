using System;

namespace SpaceWarp.API.Patching;

/// <summary>
/// This patch method/class will only be compiled (or if already compiled, used) if the mod w/ the specified GUID does not exist
/// Meant to allow for runtime compilation of patches w/ soft dependencies
/// </summary>
public class SkipOnAttribute : Attribute
{
    public string Guid;

    public SkipOnAttribute(string guid)
    {
        Guid = guid;
    }
}