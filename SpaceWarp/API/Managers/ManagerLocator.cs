using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceWarp.API.Managers;

public static class ManagerLocator
{
    private static readonly Dictionary<Type, object> Managers = new Dictionary<Type, object>();

    /// <summary>
    /// Adds a manager object to the Managers dictionary. This should only run on Start().
    /// </summary>
    /// <param name="manager">The Mod component</param>
    public static void Add(MonoBehaviour manager)
    {
        if (Managers.TryGetValue(manager.GetType(), out object _))
        {
            return;
        }

        Managers.Add(manager.GetType(), manager);
    }

    /// <summary>
    /// Tries to get a manager from the Managers dictionary.
    /// </summary>
    /// <param name="foundMod">The mod object found.</param>
    /// <typeparam name="T">The type of Mod you want to find.</typeparam>
    /// <returns></returns>
    public static bool TryGet<T>(out T foundMod) where T : MonoBehaviour
    {
        bool hasMod = Managers.TryGetValue(typeof(T), out object mod);

        foundMod = mod as T;
        return hasMod;
    }

    /// <summary>
    /// Removes a manager from the dictionary
    /// </summary>
    /// <param name="manager"></param>
    public static void Remove(Manager manager)
    {
        Managers.Remove(manager.GetType());
    }
}