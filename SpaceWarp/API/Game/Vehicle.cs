using KSP.Game;
using KSP.Sim.impl;

namespace SpaceWarp.API.Game;

public static class Vehicle
{
    public static VesselVehicle ActiveVesselVehicle => GameManager.Instance.Game.ViewController._activeVesselVehicle;
    public static VesselComponent ActiveSimVessel => GameManager.Instance.Game.ViewController.GetActiveSimVessel();
}