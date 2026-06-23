using System;
using System.Collections.Generic;
using System.IO;
using MoonSharp.Interpreter;
using ReduxLib.Configuration;
using ReduxLib.GameInterfaces;
using ReduxLib.Logging;
using SpaceWarp2.API;
using SpaceWarp2.API.Config;
using SpaceWarp2.API.Mods;

namespace SpaceWarp2.API.Lifecycle;

/// <summary>
/// The SpaceWarp-owned Lua mod runtime. Forks per-mod environments off the game's root runtime
/// (<see cref="IScriptRuntime" />), contributes the general mod-loading globals, runs registered
/// <see cref="IModEnvContributor" />s, and executes mod bodies.
/// </summary>
/// <remarks>
/// SpaceWarp owns env creation and execution. PatchManager and others contribute their own globals through
/// <see cref="IModEnvContributor" />. A mod that runs several files in one environment uses
/// <see cref="CreateModEnv" /> once and <see cref="RunBodyIn" /> per file.
/// </remarks>
public static class ModScriptRuntime
{
    /// <summary>
    /// The ModId and logger source name of the shared console environment. Console and CLI scripts run here, so
    /// their Log output is attributed to this source - the console REPL filters on it to show only that output.
    /// </summary>
    public const string ConsoleModId = "spacewarp-console";

    private static readonly ModScriptLoader Loader = new();
    private static readonly ILogger Logger = ReduxLib.ReduxLib.GetLogger("ModScriptRuntime");
    private static readonly Dictionary<string, DynValue> ConsoleGlobals = new();
    private static Table _consoleEnv;

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

        var env = CreateModEnv(descriptor);
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

        var env = CreateModEnv(descriptor);
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
    /// Forks a per-mod environment and contributes its globals, without running anything.
    /// </summary>
    /// <remarks>
    /// Reads fall through to the root globals, writes stay local to the child, so each mod is isolated on the
    /// one shared VM. The runtime contributes the general mod-loading globals (<c>ModId</c>, <c>Location</c>, the
    /// <c>require</c> loader, and the <c>Log</c> logger), then runs every registered contributor.
    /// </remarks>
    /// <param name="descriptor">The mod to fork an environment for.</param>
    /// <returns>The forked environment's globals table.</returns>
    public static Table CreateModEnv(SpaceWarpPluginDescriptor descriptor) =>
        CreateEnv(descriptor.Guid, descriptor.Folder?.FullName, descriptor.Logger, descriptor.ConfigFile);

    /// <summary>
    /// Forks the shared console environment, carrying the same globals a mod environment gets.
    /// </summary>
    /// <remarks>
    /// The in-game console runs its scripts on this environment, so a console script sees the same Lua surface
    /// (<c>SW</c>, <c>Game</c>, <c>Config</c>, contributor globals) a mod does. Its <c>Location</c> is the Lua
    /// folder, so console scripts can <c>require</c> libraries kept there.
    /// </remarks>
    /// <returns>The console environment's globals table.</returns>
    public static Table CreateConsoleEnv()
    {
        var logger = ReduxLib.ReduxLib.GetLogger(ConsoleModId);
        var configFile = new JsonConfigFile(Path.Combine(CommonPaths.LuaFolder, "spacewarp-console-config.json"));
        var env = CreateEnv(ConsoleModId, Path.GetFullPath(CommonPaths.LuaFolder), logger, configFile);
        foreach (var pair in ConsoleGlobals)
        {
            env[pair.Key] = pair.Value;
        }

        return env;
    }

    private static Table CreateEnv(string modId, string location, ILogger logger, IConfigFile configFile)
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
        SwLibrary.ContributeTo(child);
        child["Log"] = new ModLogger(logger);
        ConfigTypes.ContributeTo(child);
        child["Config"] = new ModConfig(configFile);
        child["Console"] = new ConsoleLibrary();

        foreach (var contributor in ModRuntime.Contributors)
        {
            contributor.Contribute(child);
        }

        return child;
    }

    /// <summary>
    /// Loads Lua source as a coroutine on the given environment, suspended at its start and ready to resume.
    /// </summary>
    /// <param name="env">The environment to load the coroutine on.</param>
    /// <param name="code">The Lua source to run.</param>
    /// <param name="chunkName">A name for the chunk, used in error messages.</param>
    /// <returns>The coroutine value.</returns>
    public static DynValue CreateCoroutine(Table env, string code, string chunkName)
    {
        var function = env.OwnerScript.LoadString(code, env, chunkName);
        var coroutine = env.OwnerScript.CreateCoroutine(function);
        coroutine.Coroutine.AutoYieldCounter = 1000;
        return coroutine;
    }

    /// <summary>
    /// Starts a console Lua run: a coroutine of <paramref name="code" /> on the shared console environment, which
    /// is forked on first use.
    /// </summary>
    /// <param name="code">The Lua source to run.</param>
    /// <returns>The run, which the console resumes each frame.</returns>
    public static ConsoleScriptRun RunConsole(string code)
    {
        _consoleEnv ??= CreateConsoleEnv();
        var coroutine = CreateCoroutine(_consoleEnv, code, "spacewarp-console");
        return new ConsoleScriptRun(coroutine);
    }

    /// <summary>
    /// Runs Lua synchronously on the shared console environment and returns its result, for callers that need an
    /// immediate value (such as the CLI eval bridge) rather than the frame-resumed coroutine <see cref="RunConsole" /> returns.
    /// </summary>
    /// <remarks>
    /// The console environment carries the same contributed surface a mod gets, so evaluated code sees the SW,
    /// Game, Config, and other contributor globals - not just the root globals.
    /// </remarks>
    /// <param name="code">The Lua source to evaluate.</param>
    /// <param name="chunkName">A name for the chunk, used in error messages.</param>
    /// <returns>The value the chunk returned.</returns>
    public static DynValue EvalConsoleSync(string code, string chunkName)
    {
        _consoleEnv ??= CreateConsoleEnv();
        return _consoleEnv.OwnerScript.DoString(code, _consoleEnv, chunkName);
    }

    /// <summary>
    /// Registers a value or function as a global on the console environment, so console scripts can use it.
    /// </summary>
    /// <remarks>
    /// Applies to the live console environment immediately when one exists, and is replayed onto the console
    /// environment whenever it is forked.
    /// </remarks>
    /// <param name="name">The global name.</param>
    /// <param name="value">The value or function to expose.</param>
    public static void RegisterConsoleGlobal(string name, DynValue value)
    {
        ConsoleGlobals[name] = value;
        if (_consoleEnv != null)
        {
            _consoleEnv[name] = value;
        }
    }
}
