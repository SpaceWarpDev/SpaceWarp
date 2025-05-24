using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SpaceWarp.API.Parts;

/// <summary>
/// This class allows you to register your custom PartComponentModule for background resource processing.
/// </summary>
[PublicAPI]
public static class PartComponentModuleOverride
{
    public static List<Type> RegisteredPartComponentOverrides = new();

    /// <summary>
    /// Registers your custom module for background resource processing.
    /// </summary>
    /// <typeparam name="T">Your Custom Module class that inherits from PartComponentModule</typeparam>
    public static void RegisterModuleForBackgroundResourceProcessing<T>()
    {
        var moduleName = typeof(T).Name;

        // Check if this Module is already registered
        if (RegisteredPartComponentOverrides.Contains(typeof(T)))
        {
            throw new ArgumentException(
                $"Background resource processing for module '{moduleName}' is already registered. Skipping.",
                nameof(T)
            );
        }

        RegisteredPartComponentOverrides.Add(typeof(T));
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"Registered '{moduleName}' for background resources processing.");
    }

    /// <summary>
    /// Unregisters your custom module from background resource processing.
    /// </summary>
    /// <typeparam name="T">Your Custom Module class that inherits from PartComponentModule</typeparam>
    public static void UnRegisterModuleForBackgroundResourceProcessing<T>()
    {
        if (!RegisteredPartComponentOverrides.Contains(typeof(T)))
        {
            return;
        }

        RegisteredPartComponentOverrides.Remove(typeof(T));
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"Unregistered '{typeof(T).Name}' from background resources processing.");
    }
}