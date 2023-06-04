using System;
using BepInEx.Bootstrap;
using JetBrains.Annotations;
using KSP.UI.Flight;
using MoonSharp.Interpreter;
using SpaceWarp.API.Assets;
using SpaceWarp.API.UI.Appbar;
using SpaceWarp.Backend.UI.Appbar;
using SpaceWarp.InternalUtilities;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = BepInEx.Logging.Logger;
// ReSharper disable UnusedMember.Global

namespace SpaceWarp.API.Lua;

[SpaceWarpLuaAPI("SpaceWarp")]
// ReSharper disable once UnusedType.Global
public static class SpaceWarpInterop
{
    public static LuaMod RegisterMod(string name, Table modTable)
    {
        GameObject go = new GameObject(name);
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