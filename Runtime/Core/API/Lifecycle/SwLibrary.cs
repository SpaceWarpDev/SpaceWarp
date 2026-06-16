using System;
using MoonSharp.Interpreter;
using ReduxLib.Logging;
using SpaceWarp2.API.Mods;

namespace SpaceWarp2.API.Lifecycle;

/// <summary>
/// The Lua <c>SW</c> global. Mod scripts register lifecycle closures through it (for example
/// <c>SW:Init(fn)</c>), and those closures fire at the matching SpaceWarp loading phase, before the
/// mod's C# lifecycle method. The closures are wired onto the mod's <see cref="SpaceWarpPluginDescriptor" />
/// as plain Actions, so the descriptor never has to name a game-side script type.
/// </summary>
[MoonSharpUserData]
public class SwLibrary
{
    private static readonly ILogger Logger = ReduxLib.ReduxLib.GetLogger("SpaceWarp.Lifecycle");

    static SwLibrary()
    {
        UserData.RegisterType<SwLibrary>();
    }

    /// <summary>
    /// Registers a closure to fire during the Init phase, before the mod's C# OnInitialized.
    /// </summary>
    /// <param name="context">The calling script context, used to read the mod's ModId.</param>
    /// <param name="fn">The closure to fire.</param>
    public void Init(ScriptExecutionContext context, DynValue fn) => AddHook(context, fn, postInit: false);

    /// <summary>
    /// Registers a closure to fire during the PostInit phase, before the mod's C# OnPostInitialized.
    /// </summary>
    /// <param name="context">The calling script context, used to read the mod's ModId.</param>
    /// <param name="fn">The closure to fire.</param>
    public void PostInit(ScriptExecutionContext context, DynValue fn) => AddHook(context, fn, postInit: true);

    /// <summary>
    /// Returns an extra named logger, for a mod that wants one beyond its default <c>Log</c> global.
    /// </summary>
    /// <param name="name">The logger name.</param>
    /// <returns>A logger wrapping the named ReduxLib logger.</returns>
    public ModLogger GetLogger(string name) => new(ReduxLib.ReduxLib.GetLogger(name));

    private static void AddHook(ScriptExecutionContext context, DynValue fn, bool postInit)
    {
        var phase = postInit ? "PostInit" : "Init";
        if (fn.Type != DataType.Function)
        {
            throw new ScriptRuntimeException($"SW.{phase} expects a function.");
        }

        var env = context.CurrentGlobalEnv;
        if (env == null)
        {
            throw new ScriptRuntimeException($"SW.{phase} requires a ModId on the calling environment.");
        }

        var modId = env.Get("ModId").CastToString();
        if (string.IsNullOrEmpty(modId))
        {
            throw new ScriptRuntimeException($"SW.{phase} requires a ModId on the calling environment.");
        }

        var descriptor = PluginList.TryGetDescriptor(modId);
        if (descriptor == null)
        {
            Logger.LogWarning($"SW.{phase} hook for mod '{modId}' ignored - no descriptor registered for it.");
            return;
        }

        var closure = fn.Function;
        Action hook = () => closure.Call();
        if (postInit)
        {
            descriptor.PostInitHooks.Add(hook);
        }
        else
        {
            descriptor.InitHooks.Add(hook);
        }
    }

    /// <summary>
    /// Seeds the SW global onto a mod environment's globals table. Called game-side right after the
    /// environment is forked, the one game-side touch on this surface.
    /// </summary>
    /// <param name="globals">The mod environment's globals table.</param>
    public static void SeedLifecycleGlobals(Table globals)
    {
        // Assign the raw registered object (auto-wrapped), matching how PM is seeded. Assigning a pre-made
        // UserData DynValue through the Table indexer can double-wrap and hide the methods.
        globals["SW"] = new SwLibrary();
    }
}
