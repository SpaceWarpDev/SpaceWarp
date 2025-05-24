using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReduxLib.Configuration;
using ReduxLib.Logging;

namespace SpaceWarp.Modules;

/// <summary>
/// Manager of SpaceWarp modules.
/// </summary>
public static class ModuleManager
{
    public static List<SpaceWarpModule> AllSpaceWarpModules = new();
    private static ILogger _moduleManagerLogSource;

    /// <summary>
    /// Gets a SpaceWarp module instance by name.
    /// </summary>
    /// <param name="name">Name of the module.</param>
    /// <param name="module">The module instance.</param>
    /// <returns>True if the module was found, false otherwise.</returns>
    public static bool TryGetModule(string name, out SpaceWarpModule module)
    {
        module = AllSpaceWarpModules.FirstOrDefault(x => x.Name == name);
        return module != null;
    }

    internal static void LoadAllModules()
    {
        _moduleManagerLogSource = ReduxLib.ReduxLib.GetLogger("SpaceWarp.ModuleManager");
        var configDirectory = new DirectoryInfo(Path.Combine(ReduxLib.ReduxLib.REDUX_FOLDER, "module_config"));
        _moduleManagerLogSource.LogInfo($"Redux Module Config Path: {configDirectory}");
        if (!Directory.Exists(configDirectory.FullName)) configDirectory.Create();
        // foreach (var module in modules.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly))
        // {
        //     try
        //     {
        //         var assembly = Assembly.LoadFile(module.FullName);
        //         // AllSpaceWarpModules.AddRange(assembly.GetExportedTypes()
        //         //     .Where(type => typeof(SpaceWarpModule).IsAssignableFrom(type)).Select(Activator.CreateInstance)
        //         //     .Cast<SpaceWarpModule>());
        //         foreach (var type in assembly.GetTypes().Where(type => typeof(SpaceWarpModule).IsAssignableFrom(type)))
        //         {
        //             _moduleManagerLogSource.LogInfo($"Loading module of type: {type}");
        //             var mod = (SpaceWarpModule)Activator.CreateInstance(type);
        //             _moduleManagerLogSource.LogInfo($"Module name: {mod.Name}");
        //             AllSpaceWarpModules.Add(mod);
        //         }
        //
        //         // Harmony.CreateAndPatchAll(assembly);
        //     }
        //     catch (Exception e)
        //     {
        //         _moduleManagerLogSource.LogError($"Could not load module(s) from path {module} due to error: {e}");
        //     }
        // }
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract) continue;
                if (type.IsSubclassOf(typeof(SpaceWarpModule)))
                {
                    try
                    {
                        _moduleManagerLogSource.LogInfo($"Loading module of type: {type}");
                        var mod = (SpaceWarpModule)Activator.CreateInstance(type);
                        _moduleManagerLogSource.LogInfo($"Module name: {mod.Name}");
                        AllSpaceWarpModules.Add(mod);
                    }
                    catch (Exception e)
                    {
                        _moduleManagerLogSource.LogError($"Could not load module {type} due to error: {e}");
                    }
                }
            }
        }


        TopologicallySortModules();
        List<SpaceWarpModule> toRemove = new();

        foreach (var module in AllSpaceWarpModules)
        {
            try
            {
                module.ModuleLogger = ReduxLib.ReduxLib.GetLogger(module.Name);
                module.ModuleConfiguration = new JsonConfigFile(
                    Path.Combine(configDirectory.FullName, module.Name + ".cfg")
                );
                module.LoadModule();
            }
            catch (Exception e)
            {
                _moduleManagerLogSource.LogError(
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
                _moduleManagerLogSource.LogInfo($"Pre-initializing: {module.Name}");
                module.PreInitializeModule();
            }
            catch (Exception e)
            {
                _moduleManagerLogSource.LogError(
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
                _moduleManagerLogSource.LogInfo($"Initializing: {module.Name}");
                module.InitializeModule();
            }
            catch (Exception e)
            {
                _moduleManagerLogSource.LogError(
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
                _moduleManagerLogSource.LogInfo($"Post-Initializing: {module.Name}");
                module.PostInitializeModule();
            }
            catch (Exception e)
            {
                _moduleManagerLogSource.LogError(
                    $"Error post-initializing module {module.Name} due to error: {e}.");
            }
        }
    }
}