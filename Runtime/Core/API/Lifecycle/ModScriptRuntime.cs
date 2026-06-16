using System;
using System.Collections.Generic;
using System.IO;
using MoonSharp.Interpreter;
using ReduxLib.GameInterfaces;
using ReduxLib.Logging;
using SpaceWarp2.API.Mods;

namespace SpaceWarp2.API.Lifecycle;

/// <summary>
/// The SpaceWarp-owned Lua mod runtime. Forks per-mod environments off the game's root runtime
/// (<see cref="IScriptRuntime" />), seeds the general mod-loading globals, runs registered
/// <see cref="IModEnvContributor" />s, and executes mod bodies.
/// </summary>
/// <remarks>
/// SpaceWarp owns env creation and execution. PatchManager and others contribute their own globals through
/// <see cref="IModEnvContributor" />. A mod that runs several files in one environment uses
/// <see cref="CreateModEnv" /> once and <see cref="RunBodyIn" /> per file.
/// </remarks>
public static class ModScriptRuntime
{
    private static readonly ModScriptLoader Loader = new();
    private static readonly ILogger Logger = ReduxLib.ReduxLib.GetLogger("ModScriptRuntime");

    /// <summary>
    /// Runs every script file the descriptor declared at discovery, in one forked environment.
    /// </summary>
    /// <param name="descriptor">The mod whose <see cref="SpaceWarpPluginDescriptor.ScriptFiles" /> to run.</param>
    public static void RunMod(SpaceWarpPluginDescriptor descriptor)
    {
        if (descriptor.ScriptFiles.Count == 0)
        {
            return;
        }

        var env = CreateModEnv(descriptor.Guid, descriptor.Folder?.FullName);
        foreach (var file in descriptor.ScriptFiles)
        {
            RunSource(descriptor, env, file, File.ReadAllText(file));
        }
    }

    /// <summary>
    /// Runs a set of named script sources for the descriptor in one forked environment.
    /// </summary>
    /// <remarks>
    /// For sources that are not files on disk, such as addressable text assets loaded by the owning subsystem.
    /// </remarks>
    /// <param name="descriptor">The mod the sources belong to.</param>
    /// <param name="sources">The script sources, each a (key, content) pair.</param>
    public static void RunModSources(SpaceWarpPluginDescriptor descriptor, IReadOnlyList<(string key, string content)> sources)
    {
        if (sources.Count == 0)
        {
            return;
        }

        var env = CreateModEnv(descriptor.Guid, descriptor.Folder?.FullName);
        foreach (var (key, content) in sources)
        {
            RunSource(descriptor, env, key, content);
        }
    }

    private static void RunSource(SpaceWarpPluginDescriptor descriptor, Table env, string key, string content)
    {
        try
        {
            RunBodyIn(env, content, key);
        }
        catch (Exception e)
        {
            var reason = e is InterpreterException ie ? ie.DecoratedMessage : e.ToString();
            descriptor.ScriptErrors.Add((key, reason));
            Logger.LogError($"Mod script '{key}' failed to run: {reason}");
        }

        descriptor.LoadedScripts[key] = content;
    }

    /// <summary>
    /// Forks a fresh environment for the given mod and runs a Lua body in it.
    /// </summary>
    /// <param name="modId">The mod's ID, seeded as the <c>ModId</c> global.</param>
    /// <param name="location">The mod's folder, seeded as the <c>Location</c> global, or null.</param>
    /// <param name="code">The Lua body to run.</param>
    /// <param name="chunkName">A name for the chunk, used in error messages.</param>
    public static void RunModBody(string modId, string location, string code, string chunkName)
    {
        RunBodyIn(CreateModEnv(modId, location), code, chunkName);
    }

    /// <summary>
    /// Runs a Lua body in an already-forked mod environment.
    /// </summary>
    /// <param name="env">An environment from <see cref="CreateModEnv" />.</param>
    /// <param name="code">The Lua body to run.</param>
    /// <param name="chunkName">A name for the chunk, used in error messages.</param>
    public static void RunBodyIn(Table env, string code, string chunkName)
    {
        env.OwnerScript.DoString(code, env, chunkName);
    }

    /// <summary>
    /// Forks and seeds a per-mod environment without running anything.
    /// </summary>
    /// <remarks>
    /// Reads fall through to the root globals, writes stay local to the child, so each mod is isolated on the
    /// one shared VM. The runtime seeds the general mod-loading globals (<c>ModId</c>, <c>Location</c>, the
    /// <c>require</c> loader), then runs every registered contributor.
    /// </remarks>
    /// <param name="modId">The mod's ID, seeded as the <c>ModId</c> global.</param>
    /// <param name="location">The mod's folder, seeded as the <c>Location</c> global, or null.</param>
    /// <returns>The forked environment's globals table.</returns>
    public static Table CreateModEnv(string modId, string location)
    {
        var root = IScriptRuntime.Instance.RootGlobals;
        var script = root.OwnerScript;
        script.Options.ScriptLoader = Loader;

        var child = new Table(script);
        var meta = new Table(script);
        meta["__index"] = DynValue.NewTable(root);
        child.MetaTable = meta;

        child["ModId"] = modId;
        if (location != null)
        {
            child["Location"] = location;
        }

        ModRequire.InstallOn(child);
        SwLibrary.SeedLifecycleGlobals(child);

        foreach (var contributor in ModRuntime.Contributors)
        {
            contributor.Contribute(child);
        }

        return child;
    }
}
