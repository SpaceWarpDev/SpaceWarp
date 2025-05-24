using SpaceWarp.Game.API.Waypoints;

namespace SpaceWarp.Game.API;

public interface IWaypointManager
{
    public static IWaypointManager Instance;

    public IWaypoint Get(double latitude, double longitude, double altitude,
        string? bodyName = null, string? name = null, WaypointState state = WaypointState.Visible);
    
    /// <summary>
    /// Make a waypoint from a preexisting one
    /// </summary>
    /// <param name="waypointComponent">A game waypoint component, implemented as an interface here, but this interface is only implemented on one type</param>
    /// <returns>A waypoint made from the preexisting waypoint</returns>
    public IWaypoint GetFromGameWaypointComponent(IGameWaypointComponent waypointComponent);
}