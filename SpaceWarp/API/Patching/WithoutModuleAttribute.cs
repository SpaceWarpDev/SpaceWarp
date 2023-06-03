using System;
using JetBrains.Annotations;

namespace SpaceWarp.API.Patching;

[AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
public class WithoutModuleAttribute : Attribute
{
    [CanBeNull] public string ModuleName = null;
    [CanBeNull] public Type ModuleType = null;

    public WithoutModuleAttribute(string moduleName)
    {
        ModuleName = moduleName;
    }

    public WithoutModuleAttribute(Type moduleType)
    {
        ModuleType = moduleType;
    }
}