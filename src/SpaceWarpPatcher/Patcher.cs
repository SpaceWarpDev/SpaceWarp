using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;

[assembly: InternalsVisibleTo("SpaceWarp.Core")]

namespace SpaceWarpPatcher;

[UsedImplicitly]
public static class Patcher
{
    [UsedImplicitly]
    public static IEnumerable<string> TargetDLLs => new[] { "UnityEngine.CoreModule.dll"};

    [UsedImplicitly]
    public static void Patch(ref AssemblyDefinition asm)
    {
        switch (asm.Name.Name)
        {
            // case "SpaceWarp":
            //     PatchSpaceWarp(asm);
            //     break;
            case "UnityEngine.CoreModule":
                PatchCoreModule(asm);
                break;
        }
    }

    // private static void PatchSpaceWarp(AssemblyDefinition asm)
    // {
    //     var modulePaths = $"{Paths.PluginPath}\\SpaceWarp\\modules";
    //     var modules = new DirectoryInfo(modulePaths).EnumerateFiles("*.dll").Select(x => x.FullName)
    //         .Select(AssemblyDefinition.ReadAssembly);
    //     var types = modules.SelectMany(x => x.Modules).SelectMany(x => x.GetTypes()).Where(x => x.IsPublic);
    //     var mainModule = asm.MainModule;
    //     var importedTypes = types.Select(x => mainModule.ImportReference(x));
    //     var constructor =
    //         mainModule.ImportReference(typeof(TypeForwardedToAttribute).GetConstructor(new[] { typeof(Type) }));
    //     var systemType = mainModule.ImportReference(typeof(Type));
    //
    //     CustomAttribute GetTypeForwardedToAttribute(TypeReference typeReference)
    //     {
    //         var attr = new CustomAttribute(constructor);
    //         attr.ConstructorArguments.Add(new CustomAttributeArgument(systemType,typeReference.FullName));
    //         return null;
    //     }
    //     mainModule.CustomAttributes.AddRange(importedTypes.Select(GetTypeForwardedToAttribute));
    //     asm.Write("SpaceWarpPatched.dll");
    // }

    private static void PatchCoreModule(AssemblyDefinition asm)
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

[UsedImplicitly]
internal static class Delayer
{
    [UsedImplicitly]
    public static void PatchChainloaderStart()
    {
        ChainloaderPatch.LogSource = Logger.CreateLogSource("SW BIE Extensions");
        var disabledPluginsFilepath = Path.Combine(Paths.BepInExRootPath, "disabled_plugins.cfg");
        ChainloaderPatch.DisabledPluginsFilepath = disabledPluginsFilepath;
        if (!File.Exists(disabledPluginsFilepath))
        {
            File.Create(disabledPluginsFilepath).Dispose();
            ChainloaderPatch.LogSource.LogWarning("Disabled plugins file did not exist, created empty file at: " +
                                                  disabledPluginsFilepath);
        }

        ChainloaderPatch.DisabledPluginGuids = File.ReadAllLines(disabledPluginsFilepath);
        ChainloaderPatch.DisabledPlugins = new List<PluginInfo>();
        Harmony.CreateAndPatchAll(typeof(ChainloaderPatch));
    }
}

[HarmonyPatch]
public static class ChainloaderPatch
{
    internal static string[] DisabledPluginGuids;
    public static string DisabledPluginsFilepath;
    internal static ManualLogSource LogSource;
    internal static List<PluginInfo> DisabledPlugins;
    internal static bool ModListChangedSinceLastRun;

    private static string[] AllSourceFiles(DirectoryInfo directoryInfo) =>
        directoryInfo.EnumerateFiles("*.cs", SearchOption.AllDirectories)
            .Select(fileInfo => fileInfo.FullName)
            .ToArray();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    private static void PreStartActions()
    {
        var trueLogger = Logger.CreateLogSource("Roslyn Compiler");
        var changed = CompileRoslynMods(trueLogger);
        PathsGenerator.GenerateSpaceWarpPathsDLL(changed, trueLogger);
        try
        {
            SwinfoTransformer.TransformModSwinfos();
        }
        catch (Exception e)
        {
            trueLogger.LogError(e.ToString());
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
            string hash;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(allPluginsSwinfo);
                var hashBytes = md5.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("X2"));
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
                var latestWriteTime = DateTime.FromBinary(0);


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