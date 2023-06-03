using System;

namespace SpaceWarp.API.Patching;

/// <summary>
/// Tells the patch compiler to add this module to the part and pass it in
/// </summary>

[AttributeUsage(AttributeTargets.Parameter)]
public class AddModuleAttribute : Attribute
{
    
}