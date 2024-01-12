using JetBrains.Annotations;

namespace SpaceWarp.API.Lua;

/// <summary>
/// Marks a class as a Lua API class, allowing it to be used in Lua.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class SpaceWarpLuaAPIAttribute : Attribute
{
    /// <summary>
    /// The name of the class in Lua.
    /// </summary>
    public string LuaName;

    /// <summary>
    /// Marks a class as a Lua API class, allowing it to be used in Lua.
    /// </summary>
    /// <param name="luaName"></param>
    public SpaceWarpLuaAPIAttribute(string luaName)
    {
        LuaName = luaName;
    }
}