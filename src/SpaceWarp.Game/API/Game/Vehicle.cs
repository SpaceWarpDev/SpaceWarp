using JetBrains.Annotations;
using KSP.Game;
using KSP.Sim.impl;
using SpaceWarp.API.Lua;

namespace SpaceWarp.API.Game;

/// <summary>
/// Vehicle related API.
/// </summary>
[SpaceWarpLuaAPI("Vehicle")]
[PublicAPI]
public static class Vehicle
{
    /// <summary>
    /// Gets the active vessel.
    /// </summary>
    public static VesselVehicle ActiveVesselVehicle => GameManager.Instance.Game.ViewController._activeVesselVehicle;

    /// <summary>
    /// Gets the active vessel component.
    /// </summary>
    public static VesselComponent ActiveSimVessel => GameManager.Instance.Game.ViewController.GetActiveSimVessel();
}