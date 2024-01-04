using BepInEx.Bootstrap;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using SpaceWarp.API.Logging;
using SpaceWarp.InternalUtilities;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace SpaceWarp.API.Lua;

/// <summary>
/// SpaceWarp interop class for Lua API.
/// </summary>
[SpaceWarpLuaAPI("SpaceWarp")]
[PublicAPI]
public static class SpaceWarpInterop
{
    /// <summary>
    /// Registers a Lua mod.
    /// </summary>
    /// <param name="name">Name of the mod.</param>
    /// <param name="modTable">Table containing the mod's functions.</param>
    /// <returns>The created <see cref="LuaMod"/> instance.</returns>
    public static LuaMod RegisterMod(string name, Table modTable)
    {
        var go = new GameObject(name);
        go.Persist();
        go.transform.SetParent(Chainloader.ManagerObject.transform);
        go.SetActive(false);
        var mod = go.AddComponent<LuaMod>();
        mod.Logger = new BepInExLogger(Logger.CreateLogSource(name));
        mod.ModTable = modTable;
        go.SetActive(true);
        return mod;
    }
}