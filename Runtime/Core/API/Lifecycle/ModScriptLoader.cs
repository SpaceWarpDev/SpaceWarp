using System.IO;
using System.Linq;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using SpaceWarp2.API.Mods;

namespace SpaceWarp2.API.Lifecycle;

/// <summary>
/// MoonSharp <see cref="IScriptLoader" /> for mod Lua scripts.
/// </summary>
/// <remarks>
/// Module resolution understands the <c>modGuid:moduleName</c> namespacing convention so a script can
/// <c>require</c> library files (filenames prefixed with <c>_</c>) from another mod's folder. A bare
/// <c>require("modname")</c> looks inside the host mod's own folder, identified by the script's <c>ModId</c>
/// global. File resolution joins relative paths against the script's <c>Location</c> global.
/// </remarks>
public class ModScriptLoader : IScriptLoader
{
    /// <summary>
    /// Reads the contents of a resolved Lua file.
    /// </summary>
    /// <param name="file">The full path to the file.</param>
    /// <param name="globalContext">The script's globals (unused).</param>
    /// <returns>The file's text contents.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="file" /> does not exist.</exception>
    public object LoadFile(string file, Table globalContext)
    {
        if (!File.Exists(file))
        {
            throw new ScriptRuntimeException($"File {file} does not exist.");
        }
        return File.ReadAllText(file);
    }

    [CanBeNull]
    private string GetRoot(string pluginGuid)
    {
        if (PluginList.TryGetDescriptor(pluginGuid) is not { } plugin) return null;
        return plugin.Folder.FullName;
    }

    /// <summary>
    /// Resolves a relative filename against the script's <c>Location</c> global.
    /// </summary>
    /// <param name="filename">The relative filename to resolve.</param>
    /// <param name="globalContext">The script's globals; the <c>Location</c> entry is the base directory.</param>
    /// <returns>The combined path.</returns>
    public string ResolveFileName(string filename, Table globalContext)
    {
        return Path.Combine(globalContext.Get("Location").CastToString(), filename);
    }

    [CanBeNull]
    private static string FindInFolder(string folder, string modname)
    {
        var dirInfo = new DirectoryInfo(folder);
        var needle = $"_{modname}.lua";
        return dirInfo.EnumerateFiles(needle, SearchOption.AllDirectories).Select(file => file.FullName).FirstOrDefault();
    }

    /// <summary>
    /// Resolves a <c>require</c> module name to a <c>_&lt;name&gt;.lua</c> file inside the host mod's folder, or
    /// inside another mod's folder when the name uses the <c>otherMod:moduleName</c> convention.
    /// </summary>
    /// <param name="modname">The module name passed to <c>require</c>.</param>
    /// <param name="globalContext">The script's globals; the <c>ModId</c> entry identifies the host mod for unqualified names.</param>
    /// <returns>The resolved file path, or <c>null</c> when the host mod is not registered or no matching file exists.</returns>
    public string ResolveModuleName(string modname, Table globalContext)
    {
        // Modules are namespaced mod:filename, with an _ prefix convention for local libraries.
        if (modname.Contains(':'))
        {
            var split = modname.Split(':');
            var root = GetRoot(split[0]);
            return root == null ? null : FindInFolder(root, split[1]);
        }

        var hostRoot = GetRoot(globalContext.Get("ModId").CastToString());
        return hostRoot == null ? null : FindInFolder(hostRoot, modname);
    }
}
