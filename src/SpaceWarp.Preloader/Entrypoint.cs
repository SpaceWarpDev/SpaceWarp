using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace SpaceWarp.Preloader;

internal static class Entrypoint
{
    private static readonly List<string> PreloadAssemblyPaths =
    [
        Path.Combine("KSP2_x64_Data", "Managed", "Newtonsoft.Json.dll"),
        Path.Combine("BepInEx", "core", "BepInEx.Preloader.dll"),
    ];

    private static string _gameFolder;
    private static Logger _logger;

    [UsedImplicitly]
    public static void Main(string[] args)
    {
        _gameFolder = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!;
        _logger = new Logger(_gameFolder);

        PreloadAssemblies();
        ProcessAllPatchers();
        StartBepinex();
    }

    private static void PreloadAssemblies()
    {
        foreach (var fullPath in PreloadAssemblyPaths.Select(assemblyPath => Path.Combine(_gameFolder, assemblyPath)))
        {
            try
            {
                _logger.LogDebug($"Preloading {fullPath}...");
                Assembly.LoadFile(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, $"An error occurred while preloading the assembly {fullPath}:");
            }
        }
    }

    private static void StartBepinex()
    {
        BepInEx.Preloader.Entrypoint.Main();
    }

    #region Disabling patchers of disabled plugins

    private static void ProcessAllPatchers()
    {
        var disabledPluginGuids = GetDisabledPluginGuids();

        var swinfoPaths = Directory
            .EnumerateFiles(
                Path.Combine(_gameFolder, "BepInEx", "plugins"),
                "swinfo.json",
                SearchOption.AllDirectories
            );

        var enablePatchers = new List<string>();
        var disablePatchers = new List<string>();

        foreach (var swinfoPath in swinfoPaths)
        {
            try
            {
                var (guid, patchers) = ReadSwinfo(swinfoPath);

                if (patchers == null)
                {
                    continue;
                }

                if (!disabledPluginGuids.Contains(guid))
                {
                    enablePatchers.AddRange(patchers.Select(StripExtension));
                }
                else
                {
                    disablePatchers.AddRange(patchers.Select(StripExtension));
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, $"An error occurred while processing {swinfoPath}:");
            }
        }

        RenameAllPatchers(enablePatchers, disablePatchers);
    }

    private static string[] GetDisabledPluginGuids()
    {
        var disabledPluginsPath = Path.Combine(_gameFolder, "BepInEx", "disabled_plugins.cfg");

        return File.Exists(disabledPluginsPath)
            ? File.ReadAllLines(disabledPluginsPath)
            : [];
    }

    private static (string guid, List<string> patchers) ReadSwinfo(string swinfoPath)
    {
        _logger.LogDebug($"Reading {swinfoPath}...");

        var swinfo = JObject.Parse(File.ReadAllText(swinfoPath));

        var guid = swinfo["mod_id"]?.Value<string>();
        if (guid == null)
        {
            throw new Exception($"{swinfoPath} does not contain a mod_id.");
        }

        var patchers = swinfo["patchers"]?.Values<string>().ToList();
        if (patchers == null)
        {
            _logger.LogInfo($"{guid} does not contain patchers, skipping.");
        }

        return (guid, patchers);
    }

    private static void RenameAllPatchers(ICollection<string> enablePatchers, ICollection<string> disablePatchers)
    {
        var patchers = Directory
            .EnumerateFiles(Path.Combine(_gameFolder, "BepInEx", "patchers"), "*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".dll") || file.EndsWith(".dll.disabled"));

        foreach (var patcher in patchers)
        {
            var patcherName = StripExtension(Path.GetFileName(patcher));

            if (enablePatchers.Contains(patcherName) && patcher.EndsWith(".dll.disabled"))
            {
                _logger.LogDebug($"Enabling {patcherName}...");
                File.Move(patcher, patcher.Replace(".dll.disabled", ".dll"));
            }
            else if (disablePatchers.Contains(patcherName) && patcher.EndsWith(".dll"))
            {
                _logger.LogDebug($"Disabling {patcherName}...");
                File.Move(patcher, patcher.Replace(".dll", ".dll.disabled"));
            }
            else
            {
                _logger.LogDebug($"Skipping {patcherName}...");
            }
        }
    }

    private static string StripExtension(string filename)
    {
        return filename.Replace(".disabled", "").Replace(".dll", "");
    }

    #endregion
}