using System;
using KSP.Game;
using KSP.Messages;

namespace SpaceWarp.API.Game;

public static class StateChanges
{
    #region Entering States

    public static event Action<GameStateEnteredMessage> InvalidStateEntered;
    public static event System.Action<GameStateEnteredMessage> WarmUpLoadingStateEntered;
    public static event System.Action<GameStateEnteredMessage> MainMenuStateEntered;
    public static event System.Action<GameStateEnteredMessage> KerbalSpaceCenterStateEntered;
    public static event System.Action<GameStateEnteredMessage> VehicleAssemblyBuilderEntered;
    public static event System.Action<GameStateEnteredMessage> BaseAssemblyEditorEntered;
    public static event System.Action<GameStateEnteredMessage> FlightViewEntered;
    public static event System.Action<GameStateEnteredMessage> ColonyViewEntered;
    public static event System.Action<GameStateEnteredMessage> Map3DViewEntered;
    public static event System.Action<GameStateEnteredMessage> PhotoModeEntered;
    public static event System.Action<GameStateEnteredMessage> MetricsModeEntered;
    public static event System.Action<GameStateEnteredMessage> PlanetViewerEntered;
    public static event System.Action<GameStateEnteredMessage> LoadingEntered;
    public static event System.Action<GameStateEnteredMessage> TrainingCenterEntered;
    public static event System.Action<GameStateEnteredMessage> MissionControlEntered;
    public static event System.Action<GameStateEnteredMessage> TrackingStationEntered;
    public static event System.Action<GameStateEnteredMessage> ResearchAndDevelopmentEntered;
    public static event System.Action<GameStateEnteredMessage> LaunchpadEntered;
    public static event System.Action<GameStateEnteredMessage> RunwayEntered;
    public static event System.Action<GameStateEnteredMessage> FlagEntered;

    #endregion

    #region Leaving States

    public static event System.Action<GameStateLeftMessage> InvalidStateLeft;
    public static event System.Action<GameStateLeftMessage> WarmUpLoadingStateLeft;
    public static event System.Action<GameStateLeftMessage> MainMenuStateLeft;
    public static event System.Action<GameStateLeftMessage> KerbalSpaceCenterStateLeft;
    public static event System.Action<GameStateLeftMessage> VehicleAssemblyBuilderLeft;
    public static event System.Action<GameStateLeftMessage> BaseAssemblyEditorLeft;
    public static event System.Action<GameStateLeftMessage> FlightViewLeft;
    public static event System.Action<GameStateLeftMessage> ColonyViewLeft;
    public static event System.Action<GameStateLeftMessage> PhotoModeLeft;
    public static event System.Action<GameStateLeftMessage> Map3DViewLeft;
    public static event System.Action<GameStateLeftMessage> MetricsModeLeft;
    public static event System.Action<GameStateLeftMessage> PlanetViewerLeft;
    public static event System.Action<GameStateLeftMessage> LoadingLeft;
    public static event System.Action<GameStateLeftMessage> TrainingCenterLeft;
    public static event System.Action<GameStateLeftMessage> MissionControlLeft;
    public static event System.Action<GameStateLeftMessage> TrackingStationLeft;
    public static event System.Action<GameStateLeftMessage> ResearchAndDevelopmentLeft;
    public static event System.Action<GameStateLeftMessage> LaunchpadLeft;
    public static event System.Action<GameStateLeftMessage> RunwayLeft;
    public static event System.Action<GameStateLeftMessage> FlagLeft;

    #endregion

    #region State Handling

    internal static void OnGameStateEntered(MessageCenterMessage message)
    {
        GameStateEnteredMessage msg = message as GameStateEnteredMessage;
        switch (msg!.StateBeingEntered)
        {
            case GameState.Invalid:
                InvalidStateEntered?.Invoke(msg);
                break;
            case GameState.WarmUpLoading:
                WarmUpLoadingStateEntered?.Invoke(msg);
                break;
            case GameState.MainMenu:
                MainMenuStateEntered?.Invoke(msg);
                break;
            case GameState.KerbalSpaceCenter:
                KerbalSpaceCenterStateEntered?.Invoke(msg);
                break;
            case GameState.VehicleAssemblyBuilder:
                VehicleAssemblyBuilderEntered?.Invoke(msg);
                break;
            case GameState.BaseAssemblyEditor:
                BaseAssemblyEditorEntered?.Invoke(msg);
                break;
            case GameState.FlightView:
                FlightViewEntered?.Invoke(msg);
                break;
            case GameState.ColonyView:
                ColonyViewEntered?.Invoke(msg);
                break;
            case GameState.Map3DView:
                Map3DViewEntered?.Invoke(msg);
                break;
            case GameState.PhotoMode:
                PhotoModeEntered?.Invoke(msg);
                break;
            case GameState.MetricsMode:
                MetricsModeEntered?.Invoke(msg);
                break;
            case GameState.PlanetViewer:
                PlanetViewerEntered?.Invoke(msg);
                break;
            case GameState.Loading:
                LoadingEntered?.Invoke(msg);
                break;
            case GameState.TrainingCenter:
                TrainingCenterEntered?.Invoke(msg);
                break;
            case GameState.MissionControl:
                MissionControlEntered?.Invoke(msg);
                break;
            case GameState.TrackingStation:
                TrackingStationEntered?.Invoke(msg);
                break;
            case GameState.ResearchAndDevelopment:
                ResearchAndDevelopmentEntered?.Invoke(msg);
                break;
            case GameState.Launchpad:
                LaunchpadEntered?.Invoke(msg);
                break;
            case GameState.Runway:
                RunwayEntered?.Invoke(msg);
                break;
            case GameState.Flag:
                FlagEntered?.Invoke(msg);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    internal static void OnGameStateLeft(MessageCenterMessage message)
    {
        GameStateLeftMessage msg = message as GameStateLeftMessage;
        switch (msg!.StateBeingLeft)
        {
            case GameState.Invalid:
                InvalidStateLeft?.Invoke(msg);
                break;
            case GameState.WarmUpLoading:
                WarmUpLoadingStateLeft?.Invoke(msg);
                break;
            case GameState.MainMenu:
                MainMenuStateLeft?.Invoke(msg);
                break;
            case GameState.KerbalSpaceCenter:
                KerbalSpaceCenterStateLeft?.Invoke(msg);
                break;
            case GameState.VehicleAssemblyBuilder:
                VehicleAssemblyBuilderLeft?.Invoke(msg);
                break;
            case GameState.BaseAssemblyEditor:
                BaseAssemblyEditorLeft?.Invoke(msg);
                break;
            case GameState.FlightView:
                FlightViewLeft?.Invoke(msg);
                break;
            case GameState.ColonyView:
                ColonyViewLeft?.Invoke(msg);
                break;
            case GameState.Map3DView:
                Map3DViewLeft?.Invoke(msg);
                break;
            case GameState.PhotoMode:
                PhotoModeLeft?.Invoke(msg);
                break;
            case GameState.MetricsMode:
                MetricsModeLeft?.Invoke(msg);
                break;
            case GameState.PlanetViewer:
                PlanetViewerLeft?.Invoke(msg);
                break;
            case GameState.Loading:
                LoadingLeft?.Invoke(msg);
                break;
            case GameState.TrainingCenter:
                TrainingCenterLeft?.Invoke(msg);
                break;
            case GameState.MissionControl:
                MissionControlLeft?.Invoke(msg);
                break;
            case GameState.TrackingStation:
                TrackingStationLeft?.Invoke(msg);
                break;
            case GameState.ResearchAndDevelopment:
                ResearchAndDevelopmentLeft?.Invoke(msg);
                break;
            case GameState.Launchpad:
                LaunchpadLeft?.Invoke(msg);
                break;
            case GameState.Runway:
                RunwayLeft?.Invoke(msg);
                break;
            case GameState.Flag:
                FlagLeft?.Invoke(msg);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion
}