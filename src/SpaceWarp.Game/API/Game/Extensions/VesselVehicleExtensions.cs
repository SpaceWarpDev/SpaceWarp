using JetBrains.Annotations;
using KSP.Sim.impl;
using KSP.Sim.State;
using UnityEngine;

namespace SpaceWarp.API.Game.Extensions;

[PublicAPI]
public static class VesselVehicleExtensions
{
    public static void SetMainThrottle(this VesselVehicle vehicle, float throttle)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            mainThrottle = throttle
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetRoll(this VesselVehicle vehicle, float roll)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            roll = roll
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetYaw(this VesselVehicle vehicle, float yaw)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            yaw = yaw
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetPitch(this VesselVehicle vehicle, float pitch)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            pitch = pitch
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetRollYawPitch(this VesselVehicle vehicle, float roll, float yaw, float pitch)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            roll = roll,
            yaw = yaw,
            pitch = pitch
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetRollTrim(this VesselVehicle vehicle, float rollTrim)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            rollTrim = rollTrim
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetYawTrim(this VesselVehicle vehicle, float yawTrim)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            yawTrim = yawTrim
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetPitchTrim(this VesselVehicle vehicle, float pitchTrim)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            pitchTrim = pitchTrim
        };
        vehicle.AtomicSet(incremental);
    }


    public static void SetRollYawPitchTrim(this VesselVehicle vehicle, float rollTrim, float yawTrim, float pitchTrim)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            rollTrim = rollTrim,
            yawTrim = yawTrim,
            pitchTrim = pitchTrim
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetInputRoll(this VesselVehicle vehicle, float roll)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            inputRoll = roll
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetInputYaw(this VesselVehicle vehicle, float yaw)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            inputYaw = yaw
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetInputPitch(this VesselVehicle vehicle, float pitch)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            inputPitch = pitch
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetInputRollYawPitch(this VesselVehicle vehicle, float roll, float yaw, float pitch)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            inputRoll = roll,
            inputYaw = yaw,
            inputPitch = pitch
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetWheelSteer(this VesselVehicle vehicle, float wheelSteer, float? wheelSteerTrim = null)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            wheelSteer = wheelSteer,
            wheelSteerTrim = wheelSteerTrim
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetWheelThrottle(this VesselVehicle vehicle, float wheelThrottle,
        float? wheelThrottleTrim = null)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            wheelThrottle = wheelThrottle,
            wheelThrottleTrim = wheelThrottleTrim
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetXYZ(this VesselVehicle vehicle, float? x = null, float? y = null, float? z = null)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            X = x,
            Y = y,
            Z = z
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetXYZ(this VesselVehicle vehicle, Vector3 xyz)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            X = xyz.x,
            Y = xyz.y,
            Z = xyz.z
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetKillRot(this VesselVehicle vehicle, bool killRot)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            killRot = killRot
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetGearState(this VesselVehicle vehicle, bool up)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            gearUp = up,
            gearDown = !up
        };
        vehicle.AtomicSet(incremental);
    }


    public static void SetHeadlight(this VesselVehicle vehicle, bool on)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            headlight = on
        };
        vehicle.AtomicSet(incremental);
    }


    public static void SetBrake(this VesselVehicle vehicle, bool on)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            brakes = on
        };
        vehicle.AtomicSet(incremental);
    }

    public static void SetStage(this VesselVehicle vehicle, bool stage)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            stage = stage
        };
        vehicle.AtomicSet(incremental);
    }
}