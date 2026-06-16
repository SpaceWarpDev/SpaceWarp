using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MoonSharp.Interpreter;

namespace SpaceWarp2.API.Lifecycle;

/// <summary>
/// A <c>require</c> implementation that resolves and runs modules against the calling mod's forked
/// environment instead of the shared root globals.
/// </summary>
/// <remarks>
/// MoonSharp's stock <c>require</c> resolves the module name and runs the module against the root global
/// table, so the <see cref="ModScriptLoader" /> would never see the per-mod <c>ModId</c>/<c>Location</c> and a
/// required module would lose access to that mod's globals. Installing this on each forked environment forwards
/// the environment to <see cref="Script.RequireModule" /> as its global context, which both feeds the loader
/// the right mod id and runs the module with that environment as its <c>_ENV</c> (so nested requires re-enter
/// against the same env). Results are cached per environment via a weak-keyed table, so a module shared by two
/// mods runs once per mod and the cache drops when the env is collected.
/// </remarks>
public static class ModRequire
{
    private static readonly ConditionalWeakTable<Table, Dictionary<string, DynValue>> Cache = new();

    /// <summary>
    /// Installs the mod-aware <c>require</c> as a global on the given forked environment's globals table.
    /// </summary>
    /// <param name="envGlobals">The forked environment's globals table.</param>
    public static void InstallOn(Table envGlobals)
    {
        envGlobals["require"] = DynValue.NewCallback(Require);
    }

    private static DynValue Require(ScriptExecutionContext context, CallbackArguments args)
    {
        var modname = args.AsType(0, "require", DataType.String, false).String;
        var env = context.CurrentGlobalEnv;
        var loaded = Cache.GetOrCreateValue(env);

        if (loaded.TryGetValue(modname, out var cached))
        {
            return cached;
        }

        var script = context.GetScript();
        var func = script.RequireModule(modname, env);
        var result = script.Call(func, DynValue.NewString(modname));
        if (result.IsNil())
        {
            result = DynValue.True;
        }

        loaded[modname] = result;
        return result;
    }
}
