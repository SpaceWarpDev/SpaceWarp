using System;
using KSP.Messages;

namespace SpaceWarp.API.Game.Messages;

public class StateLoadings
{
    public static event Action<TrainingCenterLoadedMessage> TrainingCenterLoaded;
    public static event Action<TrackingStationLoadedMessage> TrackingStationLoaded;
    public static event Action<TrackingStationUnloadedMessage> TrackingStationUnloaded;
    public static void TrainingCenterLoadedHandler(MessageCenterMessage message)
    {
        TrainingCenterLoaded?.Invoke(message as TrainingCenterLoadedMessage);
    }    
    public static void TrackingStationLoadedHandler(MessageCenterMessage message)
    {
        TrackingStationLoaded?.Invoke(message as TrackingStationLoadedMessage);
    }

    public static void TrackingStationUnloadedHandler(MessageCenterMessage message)
    {
        TrackingStationUnloaded?.Invoke(message as TrackingStationUnloadedMessage);
    }
    
}