using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SpaceWarp.API.Game.Waypoints;

/// <summary>
/// This contains the serialized information for a waypoint, used for saving/loading waypoints
/// </summary>
[Serializable]
[method: JsonConstructor]
[PublicAPI]
public class SerializedWaypoint(string name, string bodyName, double latitude, double longitude, double altitude, WaypointState state)
{
    /// <summary>
    /// The name of the waypoint
    /// </summary>
    public string Name => name;

    /// <summary>
    /// The body the waypoint is on
    /// </summary>
    public string BodyName => bodyName;

    /// <summary>
    /// The latitude of the waypoint
    /// </summary>
    public double Latitude => latitude;

    /// <summary>
    /// The longitude of the waypoint
    /// </summary>
    public double Longitude => longitude;

    /// <summary>
    /// The altitude of the waypoint
    /// </summary>
    public double Altitude => altitude;
    
    /// <summary>
    /// The current state of the waypoint
    /// </summary>
    public WaypointState State => state;
    
    /// <summary>
    /// Deserializes the waypoint, creating an actual waypoint from it
    /// </summary>
    /// <returns>A newly created waypoint from the serialized waypoint's parameters</returns>
    public virtual Waypoint Deserialize() => new(latitude, longitude, altitude, bodyName, name, state);
}