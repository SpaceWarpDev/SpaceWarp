using BepInEx.Bootstrap;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using SpaceWarp.InternalUtilities;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace SpaceWarp.API.Lua;

[SpaceWarpLuaAPI("SpaceWarp")]
[PublicAPI]
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