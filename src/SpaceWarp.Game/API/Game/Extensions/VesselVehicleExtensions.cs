using JetBrains.Annotations;
using KSP.Sim.impl;
using KSP.Sim.State;
using UnityEngine;

namespace SpaceWarp.API.Game.Extensions;

/// <summary>
/// Extensions for <see cref="VesselVehicle" />.
/// </summary>
[PublicAPI]
public static class VesselVehicleExtensions
{
    /// <summary>
    /// Sets the throttle of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="throttle">The throttle value.</param>
    public static void SetMainThrottle(this VesselVehicle vehicle, float throttle)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            mainThrottle = throttle
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the roll of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="roll">The roll value.</param>
    public static void SetRoll(this VesselVehicle vehicle, float roll)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            roll = roll
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the yaw of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="yaw">The yaw value.</param>
    public static void SetYaw(this VesselVehicle vehicle, float yaw)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            yaw = yaw
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the pitch of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="pitch">The pitch value.</param>
    public static void SetPitch(this VesselVehicle vehicle, float pitch)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            pitch = pitch
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the roll, yaw and pitch of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="roll">The roll value.</param>
    /// <param name="yaw">The yaw value.</param>
    /// <param name="pitch">The pitch value.</param>
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

    /// <summary>
    /// Sets the roll trim of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="rollTrim">The roll trim value.</param>
    public static void SetRollTrim(this VesselVehicle vehicle, float rollTrim)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            rollTrim = rollTrim
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the yaw trim of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="yawTrim">The yaw trim value.</param>
    public static void SetYawTrim(this VesselVehicle vehicle, float yawTrim)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            yawTrim = yawTrim
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the pitch trim of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="pitchTrim">The pitch trim value.</param>
    public static void SetPitchTrim(this VesselVehicle vehicle, float pitchTrim)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            pitchTrim = pitchTrim
        };
        vehicle.AtomicSet(incremental);
    }


    /// <summary>
    /// Sets the roll, yaw and pitch trim of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="rollTrim">The roll trim value.</param>
    /// <param name="yawTrim">The yaw trim value.</param>
    /// <param name="pitchTrim">The pitch trim value.</param>
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

    /// <summary>
    /// Sets the input roll of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="roll">The input roll value.</param>
    public static void SetInputRoll(this VesselVehicle vehicle, float roll)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            inputRoll = roll
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the input yaw of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="yaw">The input yaw value.</param>
    public static void SetInputYaw(this VesselVehicle vehicle, float yaw)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            inputYaw = yaw
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the input pitch of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="pitch">The input pitch value.</param>
    public static void SetInputPitch(this VesselVehicle vehicle, float pitch)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            inputPitch = pitch
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the input roll, yaw and pitch of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="roll">The input roll value.</param>
    /// <param name="yaw">The input yaw value.</param>
    /// <param name="pitch">The input pitch value.</param>
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

    /// <summary>
    /// Sets the wheel steer and wheel steer trim of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="wheelSteer">The wheel steer value.</param>
    /// <param name="wheelSteerTrim">The wheel steer trim value.</param>
    public static void SetWheelSteer(this VesselVehicle vehicle, float wheelSteer, float? wheelSteerTrim = null)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            wheelSteer = wheelSteer,
            wheelSteerTrim = wheelSteerTrim
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the wheel throttle and wheel throttle trim of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="wheelThrottle">The wheel throttle value.</param>
    /// <param name="wheelThrottleTrim">The wheel throttle trim value.</param>
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

    /// <summary>
    /// Sets the X, Y and Z values of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="x">The X orientation value.</param>
    /// <param name="y">The Y orientation value.</param>
    /// <param name="z">The Z orientation value.</param>
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

    /// <summary>
    /// Sets the X, Y and Z values of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="xyz">The X, Y and Z vector.</param>
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

    /// <summary>
    /// Sets the kill rotation value of the vessel.
    /// </summary>
    /// <param name="vehicle"></param>
    /// <param name="killRot"></param>
    public static void SetKillRot(this VesselVehicle vehicle, bool killRot)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            killRot = killRot
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the gear state of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="up">Whether the gear is up.</param>
    public static void SetGearState(this VesselVehicle vehicle, bool up)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            gearUp = up,
            gearDown = !up
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the headlight state of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="on">Whether the headlight is on.</param>
    public static void SetHeadlight(this VesselVehicle vehicle, bool on)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            headlight = on
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the brake state of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="on">Whether the brakes are on.</param>
    public static void SetBrake(this VesselVehicle vehicle, bool on)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            brakes = on
        };
        vehicle.AtomicSet(incremental);
    }

    /// <summary>
    /// Sets the staging state of the vessel.
    /// </summary>
    /// <param name="vehicle">The vessel.</param>
    /// <param name="stage">Whether to stage.</param>
    public static void SetStage(this VesselVehicle vehicle, bool stage)
    {
        var incremental = new FlightCtrlStateIncremental
        {
            stage = stage
        };
        vehicle.AtomicSet(incremental);
    }
}