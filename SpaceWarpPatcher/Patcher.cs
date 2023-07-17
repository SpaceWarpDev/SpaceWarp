using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

[assembly: InternalsVisibleTo("SpaceWarp")]

namespace SpaceWarpPatcher;

public static class Patcher
{
    public static IEnumerable<string> TargetDLLs => new[] { "UnityEngine.CoreModule.dll" };

    public static void Patch(AssemblyDefinition asm)
    {
        // is this necessary? I (Windows10CE) didn't think so until i had to do it!
        var targetType = asm.MainModule.GetType("UnityEngine.Application");
        var targetMethod = targetType.Methods.Single(x => x.Name == ".cctor");

        using var thisAsm = AssemblyDefinition.ReadAssembly(typeof(Patcher).Assembly.Location);
        var delayer = thisAsm.MainModule.GetType("SpaceWarpPatcher.Delayer");
        var patchMethod = delayer.Methods.Single(m => m.Name == "PatchChainloaderStart");

        ILContext il = new(targetMethod);
        ILCursor c = new(il);
        c.GotoNext(MoveType.Before,
            x => x.MatchCall("BepInEx.Bootstrap.Chainloader", "Start")
        );

        c.Emit(OpCodes.Call, il.Module.ImportReference(patchMethod));
    }
}

internal static class Delayer
{
    public static void PatchChainloaderStart()
    {
        ChainloaderPatch.LogSource = Logger.CreateLogSource("SW BIE Extensions");
        string disabledPluginsFilepath = Path.Combine(Paths.BepInExRootPath, "disabled_plugins.cfg");
        ChainloaderPatch.DisabledPluginsFilepath = disabledPluginsFilepath;
        if (!File.Exists(disabledPluginsFilepath))
        {
            File.Create(disabledPluginsFilepath).Dispose();
            ChainloaderPatch.LogSource.LogWarning("Disabled plugins file did not exist, created empty file at: " +
                                                  disabledPluginsFilepath);
        }

        ChainloaderPatch.DisabledPluginGuids = File.ReadAllLines(disabledPluginsFilepath);
        ChainloaderPatch.DisabledPlugins = new();
        Harmony.CreateAndPatchAll(typeof(ChainloaderPatch));
    }
}

[HarmonyPatch]
internal static class ChainloaderPatch
{
    internal static string[] DisabledPluginGuids;
    internal static string DisabledPluginsFilepath;
    internal static ManualLogSource LogSource;
    internal static List<PluginInfo> DisabledPlugins;
    internal static bool ModListChangedSinceLastRun;

    private static string[] AllSourceFiles(DirectoryInfo directoryInfo) =>
        directoryInfo.EnumerateFiles("*.cs", SearchOption.AllDirectories)
            .Select(fileInfo => fileInfo.FullName)
            .ToArray();

    private static bool IsDisabled(FileInfo jsonFile, string[] allDisabled)
    {
        var obj = JObject.Parse(File.ReadAllText(jsonFile.FullName));
        if (!obj.ContainsKey("spec")) return false;
        if (obj["spec"].Value<string>() is "1.2" or "1.0") return false;
        return !allDisabled.Contains(obj["mod_id"].Value<string>());
    }

    private static (string name, string path) GetNameAndPath(FileInfo jsonFile)
    {
        var path = '"' + jsonFile.Directory.FullName.Replace("\"","\\\"").Replace("\\","\\\\") + '"';
        var obj = JObject.Parse(File.ReadAllText(jsonFile.FullName));
        var id = obj["mod_id"].Value<string>();
        var replaced = id.Replace(".", "_").Replace(" ", "_");
        return (replaced, path);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    private static void PreStartActions()
    {
        var trueLogger = Logger.CreateLogSource("Roslyn Compiler");
        var changed = CompileRoslynMods(trueLogger);
        GenerateSpaceWarpPathsDLL(changed, trueLogger);
    }

    private static void GenerateSpaceWarpPathsDLL(bool changed, ManualLogSource trueLogger)
    {
        var cacheLocation = Path.Combine(Paths.BepInExRootPath, "AssemblyCache");

        try
        {
            var addressablePaths = Path.Combine(cacheLocation, "SpaceWarpPaths.dll");
            if (changed || !File.Exists(addressablePaths))
            {
                // Preload newtonsoft.json
                try
                {
                    Assembly.LoadFile(Path.Combine(Paths.ManagedPath, "Newtonsoft.Json.dll"));
                }
                catch (Exception e)
                {
                    trueLogger.LogError(e);
                }

                var disabledPluginsFilepath = Path.Combine(Paths.BepInExRootPath, "disabled_plugins.cfg");
                var allDisabled = File.ReadAllText(disabledPluginsFilepath)
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var allSwinfos =
                    new DirectoryInfo(Path.Combine(Paths.BepInExRootPath, "plugins"))
                        .EnumerateFiles("swinfo.json", SearchOption.AllDirectories).Where(x => IsDisabled(x, allDisabled))
                        .Select(x => GetNameAndPath(x));
                // Now we build the dll
                var dll = "public static class SpaceWarpPaths {\n";
                foreach (var swinfo in allSwinfos)
                {
                    dll += $"public static string {swinfo.name} = {swinfo.path};\n";
                }

                dll += "}";
                trueLogger.LogInfo($"Compiling:\n{dll}");
                var tree = CSharpSyntaxTree.ParseText(dll);
                var references = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && x.Location.Length > 0)
                    .Select(x => MetadataReference.CreateFromFile(x.Location)).ToList();
                var compilation = CSharpCompilation.Create("SpaceWarpPaths.dll", new[] { tree },references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                var result = compilation.Emit(addressablePaths);
                foreach (var diagnostic in result.Diagnostics)
                {
                    if (diagnostic.WarningLevel == 0)
                    {
                        trueLogger.LogError(diagnostic.Location + ": " + diagnostic);
                    }
                    else
                    {
                        trueLogger.LogInfo(diagnostic.Location + ": " + diagnostic);
                    }
                }

                if (!result.Success)
                {
                    try
                    {
                        File.Delete(addressablePaths);
                    }
                    catch
                    {
                        //Ignored
                    }

                }
            }

            Assembly.LoadFile(addressablePaths);
        }
        catch (Exception e)
        {
            trueLogger.LogError(e);
            //ignored
        }
    }

    private static bool CompileRoslynMods(ManualLogSource trueLogger)
    {
        try
        {
            List<string> toLoad = new()
            {
                "System.Collections.Immutable",
                "System.Memory",
                "System.Reflection.Metadata",
                "System.Threading.Tasks.Extensions",
                "Microsoft.CodeAnalysis",
                "Microsoft.CodeAnalysis.CSharp",
                "System.Runtime.CompilerServices.Unsafe",
                "System.Numerics.Vectors"
            };
            var loc = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
            foreach (var file in toLoad)
            {
                trueLogger.LogInfo($"Loading: {file}");
                Assembly.LoadFile(Path.Combine(loc, "lib", file + ".dll"));
            }

            var cacheLocation = Path.Combine(Paths.BepInExRootPath, "AssemblyCache");
            var modListHash = Path.Combine(Paths.BepInExRootPath, "ModListHash.txt");
            var disabledPluginsFilepath = Path.Combine(Paths.BepInExRootPath, "disabled_plugins.cfg");
            var allPluginsSwinfo = string.Join("",
                (new DirectoryInfo(Path.Combine(Paths.BepInExRootPath, "plugins")))
                .EnumerateFiles("swinfo.json", SearchOption.AllDirectories).Select(x => File.ReadAllText(x.FullName)));
            allPluginsSwinfo += File.ReadAllText(disabledPluginsFilepath);
            var hash = "";
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(allPluginsSwinfo);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                hash = sb.ToString();
            }
            // Then lets add the disabled plugins list (if it exists)

            if (!File.Exists(modListHash))
            {
                File.WriteAllText(modListHash, hash);
                ModListChangedSinceLastRun = true;
            }
            else
            {
                var storedHash = File.ReadAllText(modListHash);
                if (storedHash != hash)
                {
                    File.WriteAllText(modListHash, hash);
                    ModListChangedSinceLastRun = true;
                }
                else
                {
                    ModListChangedSinceLastRun = false;
                }
            }

            if (ModListChangedSinceLastRun)
            {
                if (Directory.Exists(cacheLocation))
                {
                    Directory.Delete(cacheLocation, true);
                }
            }

            if (!Directory.Exists(cacheLocation))
            {
                Directory.CreateDirectory(cacheLocation);
            }

            // Assembly.LoadFile(Path.Combine(loc, "lib", "Microsoft.CodeAnalysis.dll"));
            // Assembly.LoadFile(Path.Combine(loc, "lib", "Microsoft.CodeAnalysis.CSharp.dll"));
            // Assembly.LoadFile(Path.Combine(loc, "lib", "System.Collections.Immutable.dll"));
            trueLogger.LogInfo("Loaded assemblies");
            // So now we can compile roslyn based mods by first importing every precompiled DLL
            var pluginsFilePath = new DirectoryInfo(Path.Combine(Paths.BepInExRootPath, "plugins"));
            // So now we do a loop and generate a reference table to every plugin name that does not start with "roslyn-"
            // And we keep track of every folder that contains a src folder

            var references = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && x.Location.Length > 0)
                .Select(x => MetadataReference.CreateFromFile(x.Location)).ToList();

            foreach (var file in pluginsFilePath.EnumerateFiles("*.dll", SearchOption.AllDirectories))
            {
                if (file.Name.StartsWith("roslyn-"))
                {
                    File.Delete(file.FullName);
                }
                else
                {
                    references.Add(MetadataReference.CreateFromFile(file.FullName));
                }
            }

            foreach (var directory in pluginsFilePath.EnumerateDirectories("src", SearchOption.AllDirectories))
            {
                var parent = directory.Parent;
                if (parent == null || !File.Exists(Path.Combine(parent.FullName, "swinfo.json"))) continue;
                var id = parent.Name;


                var logger = Logger.CreateLogSource(parent.Name + " compilation");
                var allSource = AllSourceFiles(directory);
                DateTime latestWriteTime = DateTime.FromBinary(0);


                var resultFileName = "roslyn-" + id;
                var cached = Path.Combine(cacheLocation, resultFileName);
                var cachedDLL = cached + ".dll";

                var combined = Path.Combine(parent.FullName, resultFileName);
                var dll = combined + ".dll";

                if (File.Exists(cachedDLL))
                {
                    if (File.GetLastWriteTime(cachedDLL) < latestWriteTime)
                    {
                        File.Delete(cachedDLL);
                    }
                    else
                    {
                        File.Copy(cachedDLL, dll);
                        continue;
                    }
                }

                var trees = allSource.Select(x => (filename: x, text: File.ReadAllText(x))).Select(code =>
                        CSharpSyntaxTree.ParseText(code.text, CSharpParseOptions.Default, code.filename, Encoding.UTF8))
                    .ToList();
                var compilation = CSharpCompilation.Create(resultFileName + ".dll", trees, references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                var result = compilation.Emit(cachedDLL);
                foreach (var diagnostic in result.Diagnostics)
                {
                    if (diagnostic.WarningLevel == 0)
                    {
                        logger.LogError(diagnostic.Location + ": " + diagnostic);
                    }
                    else
                    {
                        logger.LogInfo(diagnostic.Location + ": " + diagnostic);
                    }
                }

                if (!result.Success)
                {
                    try
                    {
                        File.Delete(cachedDLL);
                    }
                    catch
                    {
                        //Ignored
                    }

                    continue;
                }

                File.Copy(cachedDLL, dll);
                references.Add(MetadataReference.CreateFromFile(dll));
            }
        }
        catch (Exception e)
        {
            trueLogger.LogError(e);
            trueLogger.LogInfo(e.StackTrace);
            return true;
        }

        return ModListChangedSinceLastRun;
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    private static void DisablePluginsIL(ILContext il)
    {
        ILCursor c = new(il);

        ILLabel continueLabel = default;
        c.GotoNext(MoveType.After,
            x => x.MatchBrfalse(out continueLabel), // this is from a continue, we use this to start the next iteration
            x => x.MatchLdcI4(0), // false
            x => x.MatchStloc(24) // someBool = false
        );

        c.Emit(OpCodes.Ldloc, 23); // current PluginInfo
        c.Emit(OpCodes.Ldloc, 5); // set of denied plugins so far
        // false means skip to this plugin, true means continue loading it
        c.EmitDelegate(static bool (PluginInfo plugin, HashSet<string> deniedSet) =>
        {
            if (Array.IndexOf(DisabledPluginGuids, plugin.Metadata.GUID) != -1)
            {
                deniedSet.Add(plugin.Metadata.GUID);
                DisabledPlugins.Add(plugin);
                LogSource.LogInfo($"{plugin.Metadata.GUID} was disabled, skipping loading...");
                return false;
            }

            return true;
        });
        c.Emit(OpCodes.Brfalse, continueLabel);
    }
}