using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SpaceWarpPatcher.API;

namespace SpaceWarpPatcher.Backend;

internal static class RoslynCompiler
{
    private static IEnumerable<string> AllSourceFiles(DirectoryInfo directoryInfo) => directoryInfo
        .EnumerateFiles("*.cs", SearchOption.AllDirectories)
        .Select(fileInfo => fileInfo.FullName)
        .ToArray();

    /// <summary>
    /// Compiles all Roslyn mods.
    /// </summary>
    /// <param name="trueLogger">The logger to use.</param>
    /// <returns>Whether the mod list changed since the last run.</returns>
    public static bool CompileMods(ManualLogSource trueLogger)
    {
        try
        {
            List<string> toLoad =
            [
                "System.Collections.Immutable",
                "System.Memory",
                "System.Reflection.Metadata",
                "System.Threading.Tasks.Extensions",
                "Microsoft.CodeAnalysis",
                "Microsoft.CodeAnalysis.CSharp",
                "System.Runtime.CompilerServices.Unsafe",
                "System.Numerics.Vectors"
            ];

            var loc = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent!.FullName;
            foreach (var file in toLoad)
            {
                trueLogger.LogInfo($"Loading: {file}");
                Assembly.LoadFile(Path.Combine(loc, "lib", $"{file}.dll"));
            }

            var cacheLocation = Path.Combine(Paths.BepInExRootPath, "AssemblyCache");
            var modListHash = Path.Combine(Paths.BepInExRootPath, "ModListHash.txt");
            var disabledPluginsFilepath = Path.Combine(Paths.BepInExRootPath, "disabled_plugins.cfg");

            var allPluginsSwinfo = string.Join("", new DirectoryInfo(Path.Combine(Paths.BepInExRootPath, "plugins"))
                .EnumerateFiles("swinfo.json", SearchOption.AllDirectories)
                .Select(x => File.ReadAllText(x.FullName)));
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

            if (!File.Exists(modListHash))
            {
                File.WriteAllText(modListHash, hash);
                ModList.ChangedSinceLastRun = true;
            }
            else
            {
                var storedHash = File.ReadAllText(modListHash);
                if (storedHash != hash)
                {
                    File.WriteAllText(modListHash, hash);
                    ModList.ChangedSinceLastRun = true;
                }
                else
                {
                    ModList.ChangedSinceLastRun = false;
                }
            }

            if (ModList.ChangedSinceLastRun)
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

            trueLogger.LogInfo("Loaded assemblies");
            // So now we can compile roslyn based mods by first importing every precompiled DLL
            var pluginsFilePath = new DirectoryInfo(Path.Combine(Paths.BepInExRootPath, "plugins"));
            // So now we do a loop and generate a reference table to every plugin name that does not start with "roslyn-"
            // And we keep track of every folder that contains a src folder

            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && x.Location.Length > 0)
                .Select(x => MetadataReference.CreateFromFile(x.Location))
                .ToList();

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
                if (parent == null || !File.Exists(Path.Combine(parent.FullName, "swinfo.json")))
                {
                    continue;
                }

                var id = parent.Name;

                var logger = Logger.CreateLogSource($"{parent.Name} compilation");
                var allSource = AllSourceFiles(directory);
                var latestWriteTime = DateTime.FromBinary(0);

                var resultFileName = $"roslyn-{id}";
                var cached = Path.Combine(cacheLocation, resultFileName);
                var cachedDLL = $"{cached}.dll";

                var combined = Path.Combine(parent.FullName, resultFileName);
                var dll = $"{combined}.dll";

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

                var trees = allSource.Select(x => (filename: x, text: File.ReadAllText(x)))
                    .Select(code => CSharpSyntaxTree.ParseText(
                        code.text,
                        CSharpParseOptions.Default,
                        code.filename, Encoding.UTF8
                    ))
                    .ToList();
                var compilation = CSharpCompilation.Create(
                    $"{resultFileName}.dll",
                    trees,
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                );
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

        return ModList.ChangedSinceLastRun;
    }
}