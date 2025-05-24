using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using SpaceWarp.API;

namespace SpaceWarp.InternalUtilities;

internal static class SpaceWarpPathsGenerator
{
    private static readonly Regex InvalidCharacterRegex = new("[^a-zA-Z0-9_]");
    private static readonly Regex InvalidStartRegex = new("^[0-9].*$");

    private static (string name, string path) GetNameAndPath(FileInfo jsonFile)
    {
        var path = jsonFile.Directory!.FullName;
        var obj = JObject.Parse(File.ReadAllText(jsonFile.FullName));
        var id = obj["mod_id"]!.Value<string>();
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

        if (!obj.ContainsKey("spec"))
        {
            return false;
        }

        if (obj["spec"]!.Value<string>() is "1.2" or "1.0")
        {
            return false;
        }

        return !allDisabled.Contains(obj["mod_id"]!.Value<string>());
    }

    public static Assembly GenerateSpaceWarpPathsAssembly()
    {
        var builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("SpaceWarpPaths"),AssemblyBuilderAccess.Run);
        var modBuilder = builder.DefineDynamicModule("main");
        var typeBuilder = modBuilder.DefineType("SpaceWarpPaths", TypeAttributes.Public | TypeAttributes.Class);
        
        var allSwinfoPaths = new DirectoryInfo(CommonPaths.ModsFolder)
            .EnumerateFiles("swinfo.json", SearchOption.AllDirectories)
            .Where(x => IsDisabled(x, ModList.DisabledPluginGuids));

        var allSwinfos = allSwinfoPaths.Select(GetNameAndPath);

        var swinfos = allSwinfos as (string name, string path)[] ?? allSwinfos.ToArray();
        
        foreach (var swinfo in swinfos)
        {
            var path = typeBuilder.DefineField(swinfo.name, typeof(string),
                FieldAttributes.Static | FieldAttributes.Public);
        }

        var t = typeBuilder.CreateType();

        foreach (var swinfo in swinfos)
        {
            var field = t.GetField(swinfo.name);
            field.SetValue(null, swinfo.path);
        }

        return t.Assembly;
    }
}