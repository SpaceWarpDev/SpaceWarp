using KSP.Game;
using KSP.Sim.impl;
using SpaceWarp.API.Lua;

namespace SpaceWarp.API.Game;

[SpaceWarpLuaAPI("Vehicle")]
public static class Vehicle
{
    public static VesselVehicle ActiveVesselVehicle => GameManager.Instance.Game.ViewController._activeVesselVehicle;
    public static VesselComponent ActiveSimVessel => GameManager.Instance.Game.ViewController.GetActiveSimVessel();
}