using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace SpaceWarp.API.UI.Settings;

public static class ModsPropertyDrawers
{
    private static Dictionary<Type, Func<ConfigEntryBase, GameObject>> AllPropertyDrawers = new();

    public static void AddDrawer<T>(Func<ConfigEntryBase, GameObject> drawerGenerator) => AllPropertyDrawers.Add(typeof(T),drawerGenerator);
    
    public static GameObject Drawer(ConfigEntryBase entry) =>
        AllPropertyDrawers.TryGetValue(entry.SettingType, out var func) ? func(entry) : null;
}