using System;

namespace SpaceWarp.API.Patching;

/// <summary>
/// This patch method/class will only be compiled (or if already compiled, used) if the mod w/ the specified GUID exists
/// Meant to allow for runtime compilation of patches w/ soft dependencies
/// </summary>
public class DependsOnAttribute : Attribute
{
    public string Guid;

    public DependsOnAttribute(string guid)
    {
        Guid = guid;
    }
}