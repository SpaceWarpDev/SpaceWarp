using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SpaceWarp.Game.API.Waypoints;

/// <summary>
/// This contains the serialized information for a waypoint, used for saving/loading waypoints
/// </summary>
[Serializable]
[method: JsonConstructor]
[PublicAPI]
public class SerializedWaypoint
{

    public SerializedWaypoint(string name, string bodyName, double latitude, double longitude, double altitude,
        WaypointState state)
    {
        Name = name;
        BodyName = bodyName;
        Latitude = latitude;
        Longitude = longitude;
        Altitude = altitude;
        State = state;
    }
    
    /// <summary>
    /// The name of the waypoint
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The body the waypoint is on
    /// </summary>
    public string BodyName { get; }

    /// <summary>
    /// The latitude of the waypoint
    /// </summary>
    public double Latitude { get; }

    /// <summary>
    /// The longitude of the waypoint
    /// </summary>
    public double Longitude { get; }

    /// <summary>
    /// The altitude of the waypoint
    /// </summary>
    public double Altitude { get; }
    
    /// <summary>
    /// The current state of the waypoint
    /// </summary>
    public WaypointState State { get; }
    
    /// <summary>
    /// Deserializes the waypoint, creating an actual waypoint from it
    /// </summary>
    /// <returns>A newly created waypoint from the serialized waypoint's parameters</returns>
    public virtual IWaypoint Deserialize() => IWaypointManager.Instance.Get(Latitude, Longitude, Altitude, BodyName, Name, State);
}