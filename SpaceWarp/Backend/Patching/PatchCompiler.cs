using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KSP.Modules;
using KSP.Sim.Definitions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace SpaceWarp.Backend.Patching;


internal static class PatchCompiler
{
    private static bool _generatedUsings = false;
    private static readonly List<string> NamespacesToUse = new();
    private static readonly PatchRewriter Rewriter = new();
    private static readonly List<MetadataReference> References = new();

    private static void GenerateUsingsAndReferences()
    {
        // Inject the SpaceWarp.API.Patching reference
        NamespacesToUse.Add("SpaceWarp.API.Patching");
        // Inject the KSP.Sim.Definitions reference
        NamespacesToUse.Add("KSP.Sim.Definitions");
        // Inject the System reference
        NamespacesToUse.Add("System");
        // Inject the Generic collections reference
        NamespacesToUse.Add("System.Collections.Generic");
        
        
        // Iterate over all types
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            // Only use exported types
            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.IsSubclassOf(typeof(PartBehaviourModule)))
                {
                    if (type.Namespace != null && type.Namespace.Length > 0)
                    {
                        if (!NamespacesToUse.Contains(type.Namespace))
                            NamespacesToUse.Add(type.Namespace);
                    }
                }

                if (type.IsSubclassOf(typeof(ModuleData)))
                {
                    if (type.Namespace != null && type.Namespace.Length > 0)
                    {
                        if (!NamespacesToUse.Contains(type.Namespace))
                            NamespacesToUse.Add(type.Namespace);
                    }
                }
                
                // How to allow access to configuration?
            }

            if (!assembly.IsDynamic && assembly.Location != "")
            {
                References.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        _generatedUsings = true;
    }

    private static string InjectUsingsAndNamespace(string fileText, string ns)
    {
        var lines = fileText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
            .Where(x => x.Length > 0).ToArray();
        StringBuilder sb = new StringBuilder();
        bool containsNamespace = false;
        List<string> alreadyPresentUsings = new();
        foreach (var uns in NamespacesToUse)
        {
            sb.Append($"using {uns};\n");
        }

        foreach (var line in lines)
        {
            if (line.StartsWith("namespace"))
            {
                containsNamespace = true;
            }

            sb.Append($"{line}\n");
        }

        var s = sb.ToString();
        if (!containsNamespace)
        {
            s = $"namespace {ns};\n{s}";
        }

        return s;
    }
    
    

    private static string GetNamespace(string guid, DirectoryInfo patchDirectory)
    {
        string ns = "";
        DirectoryInfo current = patchDirectory;
        while (current != null && !File.Exists(Path.Combine(current.FullName, "swinfo.json")))
        {
            if (ns.Length == 0)
            {
                ns = current.Name;
            }
            else
            {
                ns = $"{current.Name}.{ns}";
            }
            current = current.Parent;
        }

        return $"{guid}.{ns}".Replace(" ","").Replace("-","").Replace("..",".");
    }
    
    internal static Assembly CompilePatchesFor(string guid, DirectoryInfo patchDirectory)
    {
        var cacheLocation = Path.Combine(Paths.BepInExRootPath, "AssemblyCache");
        var compileLogger = Logger.CreateLogSource($"Patch Compiler: {guid}");
        if (!_generatedUsings)
        {
            GenerateUsingsAndReferences();
        }

        var dllName = GetNamespace(guid, patchDirectory) + ".dll";
        if (File.Exists(Path.Combine(cacheLocation, dllName)))
        {
            // Lets check the write data, to decide if we should invalidate the cached file
            // The cache file already gets invalidated if the number of mods has changed
            var lastWriteTime = File.GetLastWriteTime(Path.Combine(cacheLocation, dllName));
            var invalidate = patchDirectory.EnumerateFiles("*", SearchOption.AllDirectories).Where(patch => patch.Extension is "patch" or "cs").Any(patch => patch.LastWriteTime > lastWriteTime);
            invalidate = invalidate || patchDirectory.EnumerateDirectories("*", SearchOption.AllDirectories)
                .Any(dir => dir.LastWriteTime > lastWriteTime);
            if (invalidate)
            {
                File.Delete(Path.Combine(cacheLocation, dllName));
            }
            else
            {
                return Assembly.LoadFile(Path.Combine(cacheLocation, dllName));
            }
        }
        List<(string patchFileName, string code)> patchesToCompile = new();
        foreach (var patch in patchDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            if (patch.Extension != "patch" && patch.Extension != "cs")
            {
                continue;
            }
            var ns = GetNamespace(guid, patch.Directory);
            var fullName = ns + ":" + patch.Name;
            var file = File.ReadAllText(patch.FullName);
            // Inject the usings into the file
            file = InjectUsingsAndNamespace(file, ns);
            patchesToCompile.Add((fullName, file));
        }

        // This will eventually be passed on to roslyn
        List<SyntaxTree> allTrees = new();
        foreach (var patch in patchesToCompile)
        {
            var patchLogger = Logger.CreateLogSource(patch.patchFileName);
            SyntaxTree tree;
            try
            {
                tree = CSharpSyntaxTree.ParseText(patch.code);
            }
            catch (Exception e)
            {
                patchLogger.LogError($"Unable to parse patch because: {e}");
                continue;
            }

            try
            {
                var newRoot = (CSharpSyntaxNode)Rewriter.Visit(tree.GetRoot());
                tree = CSharpSyntaxTree.Create(newRoot, CSharpParseOptions.Default);
            }
            catch (Exception e)
            {
                patchLogger.LogError($"Unable to rewrite patch because: {e}");
                continue;
            }
            allTrees.Add(tree);
        }
        var compilation = CSharpCompilation.Create(dllName, allTrees, References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var result = compilation.Emit(Path.Combine(cacheLocation, dllName));
        foreach (var diagnostic in result.Diagnostics)
        {
            if (diagnostic.WarningLevel == 0)
            {
                compileLogger.LogError(diagnostic.Location + ": " + diagnostic);
            }
            else
            {
                compileLogger.LogInfo(diagnostic.Location + ": " + diagnostic);
            }
        }
        
        // How to note that this is for "this" mod, by returning it per mod
        if (result.Success) return Assembly.LoadFile(Path.Combine(cacheLocation, dllName));
        try
        {
            File.Delete(Path.Combine(cacheLocation, dllName));
        }
        catch
        {
            //Ignored
        }

        return null;
    }
}