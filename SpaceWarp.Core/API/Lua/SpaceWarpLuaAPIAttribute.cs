using System;

namespace SpaceWarp.API.Lua;

[AttributeUsage(AttributeTargets.Class)]
public class SpaceWarpLuaAPIAttribute : Attribute
{
    public string LuaName;
    public SpaceWarpLuaAPIAttribute(string luaName)
    {
        LuaName = luaName;
    }
}