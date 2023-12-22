using HarmonyLib;
using JetBrains.Annotations;
using KSP.Sim.impl;
using SpaceWarp.API.Logging;

namespace SpaceWarp.ResourceFix.API;

[PublicAPI]
public static class PartComponentModuleOverride
{
    private static readonly ILogger _LOGGER = new UnityLogSource("SpaceWarp.PartComponentModuleOverride");

    internal static List<Type> RegisteredPartComponentOverrides = new();

    /// <summary>
    /// Registers your custom module for background resource processing.
    /// </summary>
    /// <typeparam name="T">Your Custom Module class that inherits from PartComponentModule</typeparam>
    public static void RegisterModuleForBackgroundResourceProcessing<T>() where T : PartComponentModule
    {
        var moduleName = typeof(T).Name;

        // Check if this Module is already registered
        if (RegisteredPartComponentOverrides.Contains(typeof(T)))
        {
            throw new ArgumentException($"Module '{moduleName}' is already registered. Skipping.", nameof(T));
        }
        if (RegisteredPartComponentOverrides.Count == 0)
        {
            Harmony.CreateAndPatchAll(typeof(Modules.ResourceFix).Assembly,"ResourceFix");
        }
        RegisteredPartComponentOverrides.Add(typeof(T));
        _LOGGER.LogInfo($"Registered '{moduleName}' for background resources processing.");
    }

    /// <summary>
    /// Unregisters your custom module from background resource processing.
    /// </summary>
    /// <typeparam name="T">Your Custom Module class that inherits from PartComponentModule</typeparam>
    public static void UnRegisterModuleForBackgroundResourceProcessing<T>() where T : PartComponentModule
    {
        if (!RegisteredPartComponentOverrides.Contains(typeof(T))) return;

        RegisteredPartComponentOverrides.Remove(typeof(T));
        _LOGGER.LogInfo($"Unregistered '{typeof(T).Name}' from background resources processing.");
        if (RegisteredPartComponentOverrides.Count == 0)
        {
            Harmony.UnpatchID("ResourceFix");
        }
    }
}