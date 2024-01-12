using System.Reflection;
using BepInEx.Logging;
using JetBrains.Annotations;
using Mono.Cecil;
using Newtonsoft.Json.Linq;
using SpaceWarp.Preload.API;

namespace SpaceWarp.Preload;

/// <summary>
/// Patcher for the UnityEngine.CoreModule assembly.
/// </summary>
[UsedImplicitly]
public static class Entrypoint
{
    internal static ManualLogSource LogSource;

    private static readonly List<BasePatcher> Patchers = [];

    /// <summary>
    /// The target DLLs to patch.
    /// </summary>
    [UsedImplicitly]
    public static IEnumerable<string> TargetDLLs
    {
        get
        {
            LogSource = Logger.CreateLogSource("SpaceWarp.Preload");
            AddEnabledPatchers();
            return Patchers.SelectMany(patcher => patcher.DLLsToPatch).ToList();
        }
    }

    /// <summary>
    /// Patch the target assembly.
    /// </summary>
    /// <param name="asm">The target assembly.</param>
    [UsedImplicitly]
    public static void Patch(ref AssemblyDefinition asm)
    {
        var assemblyPath = Path.GetFileName(asm.MainModule.FileName);

        foreach (var patcher in Patchers.Where(patcher => patcher.DLLsToPatch.Contains(assemblyPath)))
        {
            LogSource.LogInfo($"Patching {assemblyPath} with {patcher.GetType().Name}");
            patcher.ApplyPatch(ref asm);
        }
    }

    private static void AddEnabledPatchers()
    {
        var disabledPluginGuids = GetDisabledPluginGuids();

        var swinfoPaths = Directory
            .EnumerateFiles(
                CommonPaths.BepInExPluginsPath,
                "swinfo.json",
                SearchOption.AllDirectories
            )
            .ToList();
        if (Directory.Exists(CommonPaths.InternalModLoaderPath))
        {
            swinfoPaths.AddRange(
                Directory.EnumerateFiles(
                    CommonPaths.InternalModLoaderPath,
                    "swinfo.json",
                    SearchOption.AllDirectories
                )
            );
        }

        foreach (var swinfoPath in swinfoPaths)
        {
            try
            {
                var guid = GetGuidFromSwinfo(swinfoPath);
                if (disabledPluginGuids.Contains(guid))
                {
                    continue;
                }

                // Check all DLLs in the mod folder for classes extending BasePatcher using Mono.Cecil
                // If any are found, add them to the list of patchers to enable
                var modFolder = Path.GetDirectoryName(swinfoPath)!;
                var dlls = Directory.EnumerateFiles(
                    modFolder,
                    "*.dll",
                    SearchOption.AllDirectories
                );

                foreach (var dll in dlls)
                {
                    AddPatchersFromDLL(dll);
                }

            }
            catch (Exception ex)
            {
                LogSource.LogError($"An error occurred while processing {swinfoPath}:\n{ex}");
            }
        }
    }

    private static void AddPatchersFromDLL(string dllPath)
    {
        using var asm = AssemblyDefinition.ReadAssembly(dllPath);
        var patcherTypes = asm.MainModule.Types.Where(
            type => !type.IsAbstract && type.BaseType?.FullName == "SpaceWarp.Preload.API.BasePatcher"
        );

        foreach (var patcherType in patcherTypes)
        {
            LogSource.LogInfo($"Found patcher: {patcherType.Name}");
            var loadedAsm = Assembly.LoadFile(dllPath);
            var patcher = (BasePatcher)Activator.CreateInstance(loadedAsm.GetType(patcherType.FullName));
            patcher.Logger = Logger.CreateLogSource(patcherType.Name);
            Patchers.Add(patcher);
        }
    }

    private static string[] GetDisabledPluginGuids()
    {
        var disabledPluginsPath = CommonPaths.DisabledPluginsFilepath;

        return File.Exists(disabledPluginsPath)
            ? File.ReadAllLines(disabledPluginsPath)
            : [];
    }

    private static string GetGuidFromSwinfo(string swinfoPath)
    {
        var swinfo = JObject.Parse(File.ReadAllText(swinfoPath));

        var guid = swinfo["mod_id"]?.Value<string>();
        if (guid == null)
        {
            throw new Exception($"{swinfoPath} does not contain a mod_id.");
        }

        return guid;
    }
}