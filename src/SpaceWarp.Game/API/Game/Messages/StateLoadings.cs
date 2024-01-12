using JetBrains.Annotations;
using KSP.Messages;

namespace SpaceWarp.API.Game.Messages;

/// <summary>
/// Messages related to the loading of game states.
/// </summary>
[PublicAPI]
public class StateLoadings
{
    /// <summary>
    /// Invoked when the training center is loaded.
    /// </summary>
    public static event Action<TrainingCenterLoadedMessage> TrainingCenterLoaded;
    /// <summary>
    /// Invoked when the tracking station is loaded.
    /// </summary>
    public static event Action<TrackingStationLoadedMessage> TrackingStationLoaded;
    /// <summary>
    /// Invoked when the tracking station is unloaded.
    /// </summary>
    public static event Action<TrackingStationUnloadedMessage> TrackingStationUnloaded;

    /// <summary>
    /// Handler for the <see cref="TrainingCenterLoaded"/> evenr.
    /// </summary>
    /// <param name="message">The associated message.</param>
    public static void TrainingCenterLoadedHandler(MessageCenterMessage message)
    {
        TrainingCenterLoaded?.Invoke(message as TrainingCenterLoadedMessage);
    }

    /// <summary>
    /// Handler for the <see cref="TrackingStationLoaded"/> event.
    /// </summary>
    /// <param name="message">The associated message.</param>
    public static void TrackingStationLoadedHandler(MessageCenterMessage message)
    {
        TrackingStationLoaded?.Invoke(message as TrackingStationLoadedMessage);
    }

    /// <summary>
    /// Handler for the <see cref="TrackingStationUnloaded"/> event.
    /// </summary>
    /// <param name="message">The associated message.</param>
    public static void TrackingStationUnloadedHandler(MessageCenterMessage message)
    {
        TrackingStationUnloaded?.Invoke(message as TrackingStationUnloadedMessage);
    }

}