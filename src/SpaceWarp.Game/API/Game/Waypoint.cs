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
  /// <summary>
  /// This contains the state for a waypoint
  /// </summary>
  public enum WaypointState
  {
    /// <summary>
    /// The waypoint is shown in the flight/map view
    /// </summary>
    Visible,
    /// <summary>
    /// The waypoint is hidden in the flight/map view
    /// </summary>
    Hidden
  }
  
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

  [CanBeNull] private string _hiddenRename;
  
  private WaypointState _state = WaypointState.Visible;
  /// <summary>
  /// Set the state of the waypoint to either being hidden/shown
  /// </summary>
  /// <exception cref="Exception">Thrown when trying to set the state of a destroyed waypoint</exception>
  public WaypointState State
  {
    get => _state;
    set
    {
      if (_isDestroyed)
      {
        throw new Exception("Waypoint was already destroyed");
      }
      if (value == _state) return;
      _state = value;
      if (value == WaypointState.Hidden)
      {
        _hiddenRename = Name;
        _waypointObject.Destroy();
      }
      else
      {
        var spaceSimulation = GameManager.Instance.Game.SpaceSimulation;
        var celestialBodies = GameManager.Instance.Game.UniverseModel.GetAllCelestialBodies();
        var body = celestialBodies.Find(c => c.Name == BodyName);
        if (body == null)
          throw new Exception($"Could not create waypoint as there is no body with the name of {BodyName}");
        var waypointComponentDefinition = new WaypointComponentDefinition { Name = _hiddenRename };
        _waypointObject = spaceSimulation.CreateWaypointSimObject(
          waypointComponentDefinition, body, Latitude, Longitude, AltitudeFromRadius);
        _hiddenRename = null;
      }
    }
  }
  
  private bool _isDestroyed;

  private static long _nextID;

  /// <summary>
  /// Creates a waypoint handle from a preexisting waypoint
  /// </summary>
  /// <param name="preexistingWaypoint">The preexisting waypoint</param>
  public Waypoint(WaypointComponent preexistingWaypoint)
  {
    _waypointObject = preexistingWaypoint.SimulationObject;
    var body = _waypointObject.transform.parent.transform.objectModel.CelestialBody;
    BodyName = body.Name;
    body.GetLatLonAltFromRadius(_waypointObject.transform.Position, out var latitude, out var longitude, out var altitudeFromRadius);
    Latitude = latitude;
    Longitude = longitude;
    AltitudeFromRadius = altitudeFromRadius;
  }
  
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
    if (_state != WaypointState.Visible) return;
    var bodyFrame = body.transform.bodyFrame;
    var relSurfacePosition = body.GetRelSurfacePosition(latitude, longitude, altitudeFromRadius.Value);
    _waypointObject.transform.parent = bodyFrame;
    _waypointObject.transform.Position = new Position(bodyFrame, relSurfacePosition);
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

    if (State == WaypointState.Visible)
    {
      _waypointObject.Name = name ?? $"Waypoint-{_nextID++}";
    }
    else
    {
      _hiddenRename = _waypointObject.Name;
    }
  }


  /// <summary>
  /// Hides the waypoint
  /// </summary>
  public void Hide() => State = WaypointState.Hidden;
  /// <summary>
  /// Shows the waypoint
  /// </summary>
  public void Show() => State = WaypointState.Visible;
}