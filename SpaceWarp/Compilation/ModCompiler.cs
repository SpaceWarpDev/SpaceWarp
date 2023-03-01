using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SpaceWarp.API;
using SpaceWarp.API.Logging;
using UniLinq;

namespace SpaceWarp.Compilation;

public static class ModCompiler
{
    public static readonly string CACHE_LOCATION = Path.Combine(SpaceWarpManager.SPACE_WARP_PATH,"mod_cache");

    private static BaseModLogger _logger = new ModLogger("Roslyn Compilation");
    public static Assembly CompileMod(string modID, string modSrcPath)
    {
        try
        {
            if (!Directory.Exists(modSrcPath))
            {
                return null;
            }
            _logger.Info($"starting compilation of {modID}");

            if (!CreateNewCompilation(modID, modSrcPath))
            {
                _logger.Info($"found cached version of {modID}");
                return GetCachedCompilation(modID);
            }

            _logger.Info($"no cached version for {modID}, generating assembly");

            // Now work on adding dependencies to the tree
            return CompileNewAssemblyAndCache(modID, modSrcPath);
        }
        catch (Exception e)
        {
            _logger.Error($"error compiling scripts for {modID}\n{e}");
            return null;
        }
    }
        
    private static string[] AllSourceFiles(string modSrcPath)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(modSrcPath);
        string[] sourceFiles = directoryInfo.EnumerateFiles("*.cs", SearchOption.AllDirectories)
            .Select(fileInfo => fileInfo.FullName)
            .ToArray();
        return sourceFiles;
    }

    private static bool CreateNewCompilation(string modID, string modSrcPath)
    {
        string[] allSourceFiles = AllSourceFiles(modSrcPath);
        DateTime latestWriteTime = DateTime.FromBinary(0);
        foreach (var sourceFile in allSourceFiles)
        {
            if (File.GetLastWriteTime(sourceFile) > latestWriteTime)
            {
                latestWriteTime = File.GetLastWriteTime(sourceFile);
            }
        }

        if (!Directory.Exists(CACHE_LOCATION))
        {
            Directory.CreateDirectory(CACHE_LOCATION);
            return true;
        }

        if (!File.Exists(Path.Combine(CACHE_LOCATION,modID + ".dll")))
        {
            return true;
        }

        if (File.GetLastWriteTime(Path.Combine(CACHE_LOCATION,modID + ".dll")) < latestWriteTime)
        {
            return true;
        }
            
        return false;
    }

    private static Assembly GetCachedCompilation(string modID)
    {
        return Assembly.LoadFrom(Path.Combine(CACHE_LOCATION,modID + ".dll"));
    }

    private static Assembly CompileNewAssemblyAndCache(string modID, string modSrcPath)
    {
        string[] allSourceFiles = AllSourceFiles(modSrcPath);
        List<SyntaxTree> trees = Enumerable.ToList(
            Enumerable.Select(
                Enumerable.Select(allSourceFiles, File.ReadAllText),
                code => CSharpSyntaxTree.ParseText(code)
            )
        );

        List<MetadataReference> references = Enumerable.ToList(
            Enumerable.Cast<MetadataReference>(
                from asm in AppDomain.CurrentDomain.GetAssemblies() 
                where !asm.IsDynamic 
                select MetadataReference.CreateFromFile(asm.Location)
            )
        );

        var compilation = CSharpCompilation.Create(modID + ".dll", trees, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            
        var result = compilation.Emit(Path.Combine(CACHE_LOCATION,modID + ".dll"));
        foreach (Diagnostic diagnostic in result.Diagnostics)
        {
            if (diagnostic.WarningLevel == 0)
            {
                _logger.Error(diagnostic.ToString());
            }
            else
            {
                _logger.Info(diagnostic.ToString());
            }
        }

        _logger.Info(result.ToString());
        if (!result.Success)
        {
            File.Delete(Path.Combine(CACHE_LOCATION,modID + ".dll"));
        }
        return !result.Success ? null : Assembly.LoadFile(Path.Combine(CACHE_LOCATION,modID + ".dll"));
    }
}