using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;
using UnityEngine.UIElements;

namespace SpaceWarp.Modules;

internal static class ModuleManager
{
    internal static List<SpaceWarpModule> AllSpaceWarpModules = new();
    internal static BepInExLogger ModuleManagerLogSource = new ManualLogSource("SpaceWarp.ModuleManager");

    internal static void LoadAllModules()
    {
        var location = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        var modules = new DirectoryInfo(Path.Combine(location!.FullName, "modules"));
        var configDirectory = new DirectoryInfo(Path.Combine(location!.FullName, "config"));
        foreach (var module in modules.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var assembly = Assembly.LoadFile(module.FullName);
                AllSpaceWarpModules.AddRange(assembly.GetExportedTypes()
                    .Where(type => typeof(SpaceWarpModule).IsAssignableFrom(type)).Select(Activator.CreateInstance)
                    .Cast<SpaceWarpModule>());
                Harmony.CreateAndPatchAll(assembly);
            }
            catch (Exception e)
            {
                ModuleManagerLogSource.LogError($"Could not load module(s) from path {module} due to error: {e}");
            }
        }

        
        TopologicallySortModules();
        
        foreach (var module in AllSpaceWarpModules)
        {
            try
            {
                module.ModuleLogger = (BepInExLogger)new ManualLogSource(module.Name);
                module.ModuleConfiguration =
                    new JsonConfigFile(Path.Combine(configDirectory.FullName, module.Name + ".cfg"));
                module.LoadModule();
            }
            catch (Exception e)
            {
                ModuleManagerLogSource.LogError(
                    $"Error loading module {module.Name} due to error: {e}.\n Removing {module.Name} from further initialization");
                AllSpaceWarpModules.Remove(module);
            }
        }
    }

    private static void TopologicallySortModules()
    {
        var topologicalOrder = new List<SpaceWarpModule>();
        var clone = AllSpaceWarpModules.ToList();

        var changed = true;
        while (changed)
        {
            changed = false;
            for (int i = clone.Count - 1; i >= 0; i--)
            {
                var module = clone[i];
                var resolved = module.Prerequisites.All(prerequisite => AllSpaceWarpModules.All(x => x.Name != prerequisite) || topologicalOrder.Any(x => x.Name == prerequisite));
                changed = changed || resolved;
                if (resolved)
                {
                    clone.RemoveAt(i);
                    topologicalOrder.Add(module);
                }
            }
        }
        AllSpaceWarpModules = topologicalOrder;
    }
    
    internal static void PreInitializeAllModules()
    {
        foreach (var module in AllSpaceWarpModules)
        {
            try
            {
                module.PreInitializeModule();
            }
            catch (Exception e)
            {
                ModuleManagerLogSource.LogError(
                    $"Error pre-initializing module {module.Name} due to error: {e}.\n Removing {module.Name} from further initialization");
                AllSpaceWarpModules.Remove(module);
            }
        }
    }

    internal static void InitializeAllModules()
    {
        foreach (var module in AllSpaceWarpModules)
        {
            try
            {
                module.InitializeModule();
            }
            catch (Exception e)
            {
                ModuleManagerLogSource.LogError(
                    $"Error initializing module {module.Name} due to error: {e}.\n Removing {module.Name} from further initialization");
                AllSpaceWarpModules.Remove(module);
            }
        }
    }

    internal static void PostInitializeAllModules()
    {
        foreach (var module in AllSpaceWarpModules)
        {
            try
            {
                module.PostInitializeModule();
            }
            catch (Exception e)
            {
                ModuleManagerLogSource.LogError(
                    $"Error post-initializing module {module.Name} due to error: {e}.");
            }
        }
    }
}