using System;

namespace SpaceWarp.Game.API.Waypoints;

public interface IWaypoint
{
    public string Name { get; }
    public string BodyName { get; }
    public double Latitude { get; }
    public double Longitude { get; }
    public double AltitudeFromRadius { get; }
    public WaypointState State { get; set; }
    
    /// <summary>
    /// Destroys this waypoint
    /// </summary>
    /// <exception cref="Exception">Thrown if the waypoint was already destroyed</exception>
    public void Destroy();

    
    /// <summary>
    /// Moves a waypoint to another position
    /// </summary>
    /// <param name="latitude">The new latitude of the waypoint</param>
    /// <param name="longitude">The new longitude of the waypoint</param>
    /// <param name="altitudeFromRadius">The altitude of the waypoint, if null it defaults to the height of the terrain at the specified latitude/longitude</param>
    /// <param name="bodyName">The body that the waypoint is around, if null it defaults to the waypoints current body</param>
    /// <exception cref="Exception">Thrown if the waypoint is destroyed, or if there is no waypoint with the name bodyName</exception>
    public void Move(double latitude, double longitude, double? altitudeFromRadius = null,
        string? bodyName = null);

    /// <summary>
    /// Renames a waypoint
    /// </summary>
    /// <param name="name">The new name for the waypoint, if null defaults to Waypoint-{sequential_number}</param>
    /// <exception cref="Exception">Thrown if the waypoint is destroyed</exception>
    public void Rename(string? name = null);


    /// <summary>
    /// Hides the waypoint
    /// </summary>
    public void Hide();
    
    /// <summary>
    /// Shows the waypoint
    /// </summary>
    public void Show();
    

    /// <summary>
    /// Serializes the waypoint to be saved in save data
    /// </summary>
    /// <returns>The waypoint as a serialized record</returns>
    public SerializedWaypoint Serialize();
}