using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ReduxLib.Configuration;
using UnityEngine;

namespace SpaceWarp.UI.API.Settings;

/// <summary>
/// This class is used to register custom property drawers for the settings UI.
/// </summary>
[PublicAPI]
public static class ModsPropertyDrawers
{
    private static readonly Dictionary<Type, Func<string, IConfigEntry, GameObject>> AllAbstractedPropertyDrawers = new();
    

    /// <summary>
    /// Registers a custom abstract property drawer for a specific type.
    /// </summary>
    /// <param name="drawerGenerator"></param>
    /// <typeparam name="T"></typeparam>
    public static void AddAbstractedDrawer<T>(Func<string, IConfigEntry, GameObject> drawerGenerator) =>
        AllAbstractedPropertyDrawers.Add(typeof(T), drawerGenerator);

    /// <summary>
    /// Gets a property drawer for a config entry with a name.
    /// </summary>
    /// <param name="name">The name of the config entry.</param>
    /// <param name="entry">The config entry to get a drawer for.</param>
    /// <returns></returns>
    public static GameObject Drawer(string name, IConfigEntry entry)
    {
        if (entry.ValueType.IsEnum && !AllAbstractedPropertyDrawers.ContainsKey(entry.ValueType))
            AllAbstractedPropertyDrawers.Add(entry.ValueType, EnumDrawerGenerator(entry.ValueType));
        if (!AllAbstractedPropertyDrawers.ContainsKey(entry.ValueType))
        {
            try
            {
                AllAbstractedPropertyDrawers.Add(entry.ValueType,GenericDrawerGenerator(entry.ValueType));
            }
            catch
            {
                //Ignored
            }
        }
        return AllAbstractedPropertyDrawers.TryGetValue(entry.ValueType, out var func) ? func(name,entry) : null;
    }

    public static Func<Type, Func<string, IConfigEntry, GameObject>> EnumDrawerGenerator;
    public static Func<Type, Func<string, IConfigEntry, GameObject>> GenericDrawerGenerator;
}