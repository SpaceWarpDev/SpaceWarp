using System;

namespace SpaceWarp.API.Patching;


// Marks a part patch function
[AttributeUsage(AttributeTargets.Method)]
public class PartPatchAttribute : Attribute
{
    
}