using System;

namespace SpaceWarp.API.Patching;

[AttributeUsage(AttributeTargets.Class)]
public class WithNameAttribute : Attribute
{
    public string Name;

    public WithNameAttribute(string name)
    {
        Name = name;
    }
}