using System;
using System.Collections.Generic;

namespace SpaceWarp.API.Mods;

/// <summary>
/// Useful performance-optimized locator for Mod objects. Should be used instead of any other way.
/// </summary>
public static class ModLocator
{
    private static readonly Dictionary<Type, object> Mods = new Dictionary<Type, object>();

    /// <summary>
    /// Adds a mod object to the Mods dictionary. This should only run on Mod.Start().
    /// </summary>
    /// <param name="modObject">The Mod component</param>
    public static void Add(Mod modObject)
    {
        if (Mods.TryGetValue(modObject.GetType(), out object _))
        {
            return;
        }

        Mods.Add(modObject.GetType(), modObject);
    }

    /// <summary>
    /// Tries to get a mod from the Mods dictionary.
    /// </summary>
    /// <param name="foundMod">The mod object found.</param>
    /// <typeparam name="T">The type of Mod you want to find.</typeparam>
    /// <returns></returns>
    public static bool TryGet<T>(out T foundMod) where T : Mod
    {
        bool hasMod = Mods.TryGetValue(typeof(T), out object mod);

        foundMod = mod as T;
        return hasMod;
    }

    /// <summary>
    /// Removes a manager from the dictionary
    /// </summary>
    /// <param name="manager"></param>
    public static void Remove(Mod modObject)
    {
        Mods.Remove(modObject.GetType());
    }
}