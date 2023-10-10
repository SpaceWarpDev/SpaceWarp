using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;

namespace SpaceWarpPatcher;

internal static class PathsGenerator
{


    private static Regex InvalidCharacterRegex = new(@"[^a-zA-Z0-9_]");
    private static Regex InvalidStartRegex = new(@"^[0-9].*$");
    private static (string name, string path) GetNameAndPath(FileInfo jsonFile)
    {
        var path = '"' + jsonFile.Directory.FullName.Replace("\"","\\\"").Replace("\\","\\\\") + '"';
        var obj = JObject.Parse(File.ReadAllText(jsonFile.FullName));
        var id = obj["mod_id"].Value<string>();
        // var replaced = id.Replace(".", "_").Replace(" ", "_").Replace("-","_");
        var replaced = InvalidCharacterRegex.Replace(id, "_");
        if (InvalidStartRegex.IsMatch(replaced))
        {
            replaced = $"_{replaced}";
        }
        return (replaced, path);
    }
    private static bool IsDisabled(FileInfo jsonFile, string[] allDisabled)
    {
        var obj = JObject.Parse(File.ReadAllText(jsonFile.FullName));
        if (!obj.ContainsKey("spec")) return false;
        if (obj["spec"].Value<string>() is "1.2" or "1.0") return false;
        return !allDisabled.Contains(obj["mod_id"].Value<string>());
    }
    internal static void GenerateSpaceWarpPathsDLL(bool changed, ManualLogSource trueLogger)
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
                var allSwinfoPaths =
                    new DirectoryInfo(Path.Combine(Paths.BepInExRootPath, "plugins"))
                        .EnumerateFiles("swinfo.json", SearchOption.AllDirectories).Where(x => IsDisabled(x, allDisabled));
                var gameDataMods = new DirectoryInfo(Path.Combine(Paths.GameRootPath, "GameData", "Mods"));
                if (gameDataMods.Exists)
                {
                    allSwinfoPaths =
                        allSwinfoPaths.Concat(
                            new DirectoryInfo(Path.Combine(Paths.GameRootPath, "GameData", "Mods")).EnumerateFiles(
                                "swinfo.json", SearchOption.AllDirectories));
                }
                var allSwinfos = allSwinfoPaths.Select(GetNameAndPath);
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
}