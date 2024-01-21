using JetBrains.Annotations;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;

namespace SpaceWarp.API.Game;

/// <summary>
/// A handle for a waypoint in the flight/map view for KSP2
/// </summary>
[PublicAPI]
public class Waypoint {
  private SimulationObjectModel _waypointObject;
  /// <summary>
  /// The current name of the waypoint
  /// </summary>
  public string Name => _waypointObject.Name;

  /// <summary>
  /// The current body that the waypoint is placed on
  /// </summary>
  public string BodyName { get; private set; }

  /// <summary>
  /// The current latitude of the waypoint
  /// </summary>
  public double Latitude { get; private set; }

  /// <summary>
  /// The current longitude of the waypoint
  /// </summary>
  public double Longitude { get; private set; }

  /// <summary>
  /// The current altitude of the waypoint
  /// </summary>
  public double AltitudeFromRadius { get; private set; }

  private bool _isDestroyed;

  private static long _nextID;
  
  /// <summary>
  /// Create a new waypoint at the specified location
  /// </summary>
  /// <param name="latitude">The latitude of the waypoint</param>
  /// <param name="longitude">The longitude of the waypoint</param>
  /// <param name="altitudeFromRadius">The altitude of the waypoint, if null it defaults to the height of the terrain at the specified latitude/longitude</param>
  /// <param name="bodyName">The body that the waypoint is around, if null it defaults to the current active vehicles body</param>
  /// <param name="name">The name of the waypoint, if null defaults to Waypoint-{sequential_number}</param>
  /// <exception cref="Exception">Thrown if there is no body with the name bodyName</exception>
  public Waypoint(double latitude, double longitude, double? altitudeFromRadius = null, [CanBeNull] string bodyName = null, [CanBeNull] string name = null) {
    BodyName = bodyName ??= Vehicle.ActiveSimVessel.mainBody.Name;
    Latitude = latitude;
    Longitude = longitude;
    var spaceSimulation = GameManager.Instance.Game.SpaceSimulation;
    var celestialBodies = GameManager.Instance.Game.UniverseModel.GetAllCelestialBodies();
    var body = celestialBodies.Find(c => c.Name == bodyName);
    if (body == null)
        throw new Exception($"Could not create waypoint as there is no body with the name of {bodyName}");
    altitudeFromRadius ??= body.SurfaceProvider.GetTerrainAltitudeFromCenter(latitude, longitude) - body.radius;
    AltitudeFromRadius = altitudeFromRadius.Value;
    var waypointComponentDefinition = new WaypointComponentDefinition { Name = name ?? $"Waypoint-{_nextID++}" };
    _waypointObject = spaceSimulation.CreateWaypointSimObject(
        waypointComponentDefinition, body, latitude, longitude, altitudeFromRadius.Value);
  }

  /// <summary>
  /// Destroys this waypoint
  /// </summary>
  /// <exception cref="Exception">Thrown if the waypoint was already destroyed</exception>
  public void Destroy() {
    if (_isDestroyed) {
      throw new Exception("Waypoint was already destroyed");
    }
    _waypointObject.Destroy();
    _isDestroyed = true;
  }
  
  /// <summary>
  /// Moves a waypoint to another position
  /// </summary>
  /// <param name="latitude">The new latitude of the waypoint</param>
  /// <param name="longitude">The new longitude of the waypoint</param>
  /// <param name="altitudeFromRadius">The altitude of the waypoint, if null it defaults to the height of the terrain at the specified latitude/longitude</param>
  /// <param name="bodyName">The body that the waypoint is around, if null it defaults to the waypoints current body</param>
  /// <exception cref="Exception">Thrown if the waypoint is destroyed, or if there is no waypoint with the name bodyName</exception>
  public void Move(double latitude, double longitude, double? altitudeFromRadius = null, [CanBeNull] string bodyName = null) {
    if (_isDestroyed) {
      throw new Exception("Waypoint was already destroyed");
    }
    bodyName ??= BodyName;
    var celestialBodies = GameManager.Instance.Game.UniverseModel.GetAllCelestialBodies();
    var body = celestialBodies.Find(c => c.Name == bodyName);
    if (body == null)
        throw new Exception($"Could not create waypoint as there is no body with the name of {bodyName}");
    altitudeFromRadius ??= body.SurfaceProvider.GetTerrainAltitudeFromCenter(latitude, longitude) - body.radius;
    BodyName = bodyName;
    Latitude = latitude;
    Longitude = longitude;
    AltitudeFromRadius = altitudeFromRadius.Value;
    var bodyFrame = body.transform.bodyFrame;
    var relSurfacePosition = body.GetRelSurfacePosition(latitude,longitude,altitudeFromRadius.Value);
    _waypointObject.transform.parent = bodyFrame;
    _waypointObject.transform.Position = new Position(bodyFrame,relSurfacePosition);
  }
  
  /// <summary>
  /// Renames a waypoint
  /// </summary>
  /// <param name="name">The new name for the waypoint, if null defaults to Waypoint-{sequential_number}</param>
  /// <exception cref="Exception">Thrown if the waypoint is destroyed</exception>
  public void Rename([CanBeNull] string name = null) {
    if (_isDestroyed) {
      throw new Exception("Waypoint was already destroyed");
    }
    _waypointObject.Name = name ?? $"Waypoint-{_nextID++}";
  }

  ~Waypoint()
  {
    Destroy();
  }
}