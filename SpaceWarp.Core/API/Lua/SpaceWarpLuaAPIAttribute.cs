using System;
using JetBrains.Annotations;

namespace SpaceWarp.API.Lua;

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class SpaceWarpLuaAPIAttribute : Attribute
{
    public string LuaName;
    public SpaceWarpLuaAPIAttribute(string luaName)
    {
        LuaName = luaName;
    }
}