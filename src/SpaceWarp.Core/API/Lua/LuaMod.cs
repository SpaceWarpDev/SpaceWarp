using BepInEx.Logging;
using JetBrains.Annotations;
using KSP.Game;
using MoonSharp.Interpreter;

namespace SpaceWarp.API.Lua;

/// <summary>
/// A Lua mod, this is the base class for all mods that are written in Lua.
/// </summary>
[MoonSharpUserData]
[PublicAPI]
public class LuaMod : KerbalMonoBehaviour
{
    /// <summary>
    /// The table that contains the mod's functions.
    /// </summary>
    public Table ModTable;
    // TODO: Add more than just this to the behaviour but for now

    #region Message Definitions
    // Start
    private Closure _start;

    // Update Functions
    private Closure _update;
    private Closure _fixedUpdate;
    private Closure _lateUpdate;

    // Enable/Disable
    private Closure _onEnable;
    private Closure _onDisable;

    // Destruction
    private Closure _onDestroy;

    // Reset
    private Closure _reset;
    #endregion

    /// <summary>
    /// The logger for this mod.
    /// </summary>
    public ManualLogSource Logger;

    /// <summary>
    /// A pass through to the wrapped table
    /// </summary>
    /// <param name="idx">The index of the name.</param>
    public DynValue this[DynValue idx]
    {
        get => ModTable.Get(idx);
        set => ModTable[idx] = value;
    }

    private void TryCallMethod(Closure closure, params object[] args)
    {
        try
        {
            closure.Call(args);
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }
    #region Message Handlers

    private void TryRegister(string methodName, out Closure method)
    {
        if (ModTable.Get(methodName) != null && ModTable.Get(methodName).Type == DataType.Function)
        {
            method = ModTable.Get(methodName).Function;
            return;
        }
        method = null;
    }

    private void Awake()
    {
        if (ModTable.Get("Awake") != null && ModTable.Get("Awake").Type == DataType.Function)
        {
            var awakeFunction = ModTable.Get("Awake").Function;
            TryCallMethod(awakeFunction);
        }

        TryRegister(nameof(Start),out _start);

        TryRegister(nameof(Update), out _update);

        TryRegister(nameof(LateUpdate), out _lateUpdate);

        TryRegister(nameof(FixedUpdate), out _fixedUpdate);

        TryRegister(nameof(OnEnable), out _onEnable);

        TryRegister(nameof(OnDisable), out _onDisable);

        TryRegister(nameof(OnDestroy), out _onDestroy);

        TryRegister(nameof(Reset), out _reset);

    }

    // Start
    private void Start()
    {
        if (_start != null)
        {
            TryCallMethod(_start, this);
        }
    }

    // Update Functions

    private void Update()
    {
        if (_update != null)
        {
            TryCallMethod(_update, this);
        }
    }

    private void FixedUpdate()
    {
        if (_fixedUpdate != null)
        {
            TryCallMethod(_fixedUpdate, this);
        }
    }

    private void LateUpdate()
    {
        if (_lateUpdate != null)
        {
            TryCallMethod(_lateUpdate, this);
        }
    }

    // Enable/Disable

    private void OnEnable()
    {
        if (_onEnable != null)
        {
            TryCallMethod(_onEnable, this);
        }
    }

    private void OnDisable()
    {
        if (_onDisable != null)
        {
            TryCallMethod(_onDisable, this);
        }
    }

    // Destruction

    private void OnDestroy()
    {
        if (_onDestroy != null)
        {
            TryCallMethod(_onDestroy, this);
        }
    }

    // Reset
    private void Reset()
    {
        if (_reset != null)
        {
            TryCallMethod(_reset, this);
        }
    }

    #endregion
}