using BepInEx.Bootstrap;
using MoonSharp.Interpreter;
using SpaceWarp.InternalUtilities;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;
// ReSharper disable UnusedMember.Global

namespace SpaceWarp.API.Lua;

[SpaceWarpLuaAPI("SpaceWarp")]
// ReSharper disable once UnusedType.Global
public static class SpaceWarpInterop
{
    public static LuaMod RegisterMod(string name, Table modTable)
    {
        var go = new GameObject(name);
        go.Persist();
        go.transform.SetParent(Chainloader.ManagerObject.transform);
        go.SetActive(false);
        var mod = go.AddComponent<LuaMod>();
        mod.Logger = Logger.CreateLogSource(name);
        mod.ModTable = modTable;
        go.SetActive(true);
        return mod;
    }
}