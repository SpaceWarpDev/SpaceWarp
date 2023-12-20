using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;
using ILogger = SpaceWarp.API.Logging.ILogger;

namespace SpaceWarp.Modules;

public static class ModuleManager
{
    internal static List<SpaceWarpModule> AllSpaceWarpModules = new();
    private static readonly ILogger ModuleManagerLogSource = new UnityLogSource("SpaceWarp.ModuleManager");

    public static bool TryGetModule(string name, out SpaceWarpModule module)
    {
        module = AllSpaceWarpModules.FirstOrDefault(x => x.Name == name);
        return module != null;
    }

    internal static void LoadAllModules()
    {
        var location = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        var modules = new DirectoryInfo(Path.Combine(location!.FullName, "modules"));
        ModuleManagerLogSource.LogInfo($"Modules location: {modules}");
        var configDirectory = new DirectoryInfo(Path.Combine(location!.FullName, "config"));
        if (!Directory.Exists(configDirectory.FullName)) configDirectory.Create();
        foreach (var module in modules.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var assembly = Assembly.LoadFile(module.FullName);
                // AllSpaceWarpModules.AddRange(assembly.GetExportedTypes()
                //     .Where(type => typeof(SpaceWarpModule).IsAssignableFrom(type)).Select(Activator.CreateInstance)
                //     .Cast<SpaceWarpModule>());
                foreach (var type in assembly.GetTypes().Where(type => typeof(SpaceWarpModule).IsAssignableFrom(type)))
                {
                    ModuleManagerLogSource.LogInfo($"Loading module of type: {type}");
                    var mod = (SpaceWarpModule)Activator.CreateInstance(type);
                    ModuleManagerLogSource.LogInfo($"Module name: {mod.Name}");
                    AllSpaceWarpModules.Add(mod);
                }

                Harmony.CreateAndPatchAll(assembly);
            }
            catch (Exception e)
            {
                ModuleManagerLogSource.LogError($"Could not load module(s) from path {module} due to error: {e}");
            }
        }


        TopologicallySortModules();
        List<SpaceWarpModule> toRemove = new();

        foreach (var module in AllSpaceWarpModules)
        {
            try
            {
                module.ModuleLogger = new UnityLogSource(module.Name);
                module.ModuleConfiguration = new JsonConfigFile(
                    Path.Combine(configDirectory.FullName, module.Name + ".cfg")
                );
                module.LoadModule();
            }
            catch (Exception e)
            {
                ModuleManagerLogSource.LogError(
                    $"Error loading module {module.Name} due to error: {e}.\n Removing {module.Name} from further initialization");
                toRemove.Add(module);
            }
        }

        foreach (var module in toRemove)
        {
            AllSpaceWarpModules.Remove(module);
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
            for (var i = clone.Count - 1; i >= 0; i--)
            {
                var module = clone[i];
                var resolved = module.Prerequisites.All(prerequisite =>
                    AllSpaceWarpModules.All(x => x.Name != prerequisite) ||
                    topologicalOrder.Any(x => x.Name == prerequisite)
                );
                changed = changed || resolved;
                if (!resolved) continue;
                clone.RemoveAt(i);
                topologicalOrder.Add(module);
            }
        }

        AllSpaceWarpModules = topologicalOrder;
    }

    internal static void PreInitializeAllModules()
    {
        List<SpaceWarpModule> toRemove = new();
        foreach (var module in AllSpaceWarpModules)
        {
            try
            {
                ModuleManagerLogSource.LogInfo($"Pre-initializing: {module.Name}");
                module.PreInitializeModule();
            }
            catch (Exception e)
            {
                ModuleManagerLogSource.LogError(
                    $"Error pre-initializing module {module.Name} due to error: {e}.\n Removing {module.Name} from further initialization");
                toRemove.Add(module);
            }
        }

        foreach (var module in toRemove)
        {
            AllSpaceWarpModules.Remove(module);
        }
    }

    internal static void InitializeAllModules()
    {
        List<SpaceWarpModule> toRemove = new();
        foreach (var module in AllSpaceWarpModules)
        {
            try
            {
                ModuleManagerLogSource.LogInfo($"Initializing: {module.Name}");
                module.InitializeModule();
            }
            catch (Exception e)
            {
                ModuleManagerLogSource.LogError(
                    $"Error initializing module {module.Name} due to error: {e}.\n Removing {module.Name} from further initialization");
                toRemove.Add(module);
            }
        }

        foreach (var module in toRemove)
        {
            AllSpaceWarpModules.Remove(module);
        }
    }

    internal static void PostInitializeAllModules()
    {
        foreach (var module in AllSpaceWarpModules)
        {
            try
            {
                ModuleManagerLogSource.LogInfo($"Post-Initializing: {module.Name}");
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