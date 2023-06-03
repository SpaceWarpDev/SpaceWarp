using System;
using JetBrains.Annotations;

namespace SpaceWarp.API.Patching;

[AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
public class WithModuleAttribute : Attribute
{
    [CanBeNull] public string ModuleName = null;
    [CanBeNull] public Type ModuleType = null;

    public WithModuleAttribute(string moduleName)
    {
        ModuleName = moduleName;
    }

    public WithModuleAttribute(Type moduleType)
    {
        ModuleType = moduleType;
    }
}