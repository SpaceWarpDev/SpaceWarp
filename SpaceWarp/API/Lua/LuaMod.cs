
using System;
using BepInEx.Logging;
using KSP.Game;
using MoonSharp.Interpreter;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace SpaceWarp.API.Lua;



[MoonSharpUserData]
public class LuaMod : KerbalMonoBehaviour
{
    public Table ModTable;
    // TODO: Add more than just this to the behaviour but for now
    
    #region Message Definitions
    // Start
    private Closure _start = null;

    // Update Functions
    private Closure _update = null;
    private Closure _fixedUpdate = null;
    private Closure _lateUpdate = null;
    
    // Enable/Disable
    private Closure _onEnable = null;
    private Closure _onDisable = null;
    
    // Destruction
    private Closure _onDestroy = null;
    
    // Reset
    private Closure _reset = null;
    #endregion
    
    public ManualLogSource Logger;

    // First a pass through to the wrapped table
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

    private void TryRegister(string name, out Closure method)
    {
        if (ModTable.Get(name) != null && ModTable.Get(name).Type == DataType.Function)
        {
            method = ModTable.Get(name).Function;
            return;
        }
        method = null;
    }
    
    public void Awake()
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
    public void Start()
    {
        if (_start != null)
        {
            TryCallMethod(_start, this);
        }
    }

    // Update Functions

    public void Update()
    {
        if (_update != null)
        {
            TryCallMethod(_update, this);
        }
    }

    public void FixedUpdate()
    {
        if (_fixedUpdate != null)
        {
            TryCallMethod(_fixedUpdate, this);
        }
    }

    public void LateUpdate()
    {
        if (_lateUpdate != null)
        {
            TryCallMethod(_lateUpdate, this);
        }
    }
    
    // Enable/Disable

    public void OnEnable()
    {
        if (_onEnable != null)
        {
            TryCallMethod(_onEnable, this);
        }
    }

    public void OnDisable()
    {
        if (_onDisable != null)
        {
            TryCallMethod(_onDisable, this);
        }
    }
    
    // Destruction

    public void OnDestroy()
    {
        if (_onDestroy != null)
        {
            TryCallMethod(_onDestroy, this);
        }
    }

    // Reset
    public void Reset()
    {
        if (_reset != null)
        {
            TryCallMethod(_reset, this);
        }
    }

    #endregion
}