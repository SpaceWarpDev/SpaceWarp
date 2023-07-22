using System;
using KSP.Game;
using KSP.Messages;

namespace SpaceWarp.API.Game.Messages;

/// <summary>
///     A class that contains a list of events that are published either when a state is entered or left
/// </summary>
public static class StateChanges
{
    #region State changing

    /// <summary>
    ///     Invoked when the game state is changed
    ///     <para>Action(Message,PreviousState,CurrentState)</para>
    /// </summary>
    public static event Action<GameStateChangedMessage, GameState, GameState> GameStateChanged;

    #endregion

    #region Entering States

    public static event Action<GameStateEnteredMessage> InvalidStateEntered;
    public static event Action<GameStateEnteredMessage> WarmUpLoadingStateEntered;
    public static event Action<GameStateEnteredMessage> MainMenuStateEntered;
    public static event Action<GameStateEnteredMessage> KerbalSpaceCenterStateEntered;
    public static event Action<GameStateEnteredMessage> VehicleAssemblyBuilderEntered;
    public static event Action<GameStateEnteredMessage> BaseAssemblyEditorEntered;
    public static event Action<GameStateEnteredMessage> FlightViewEntered;
    public static event Action<GameStateEnteredMessage> ColonyViewEntered;
    public static event Action<GameStateEnteredMessage> Map3DViewEntered;
    public static event Action<GameStateEnteredMessage> PhotoModeEntered;
    public static event Action<GameStateEnteredMessage> MetricsModeEntered;
    public static event Action<GameStateEnteredMessage> PlanetViewerEntered;
    public static event Action<GameStateEnteredMessage> LoadingEntered;
    public static event Action<GameStateEnteredMessage> TrainingCenterEntered;
    public static event Action<GameStateEnteredMessage> MissionControlEntered;
    public static event Action<GameStateEnteredMessage> TrackingStationEntered;
    public static event Action<GameStateEnteredMessage> ResearchAndDevelopmentEntered;
    public static event Action<GameStateEnteredMessage> LaunchpadEntered;
    public static event Action<GameStateEnteredMessage> RunwayEntered;
    public static event Action<GameStateEnteredMessage> FlagEntered;

    #endregion

    #region Leaving States

    public static event Action<GameStateLeftMessage> InvalidStateLeft;
    public static event Action<GameStateLeftMessage> WarmUpLoadingStateLeft;
    public static event Action<GameStateLeftMessage> MainMenuStateLeft;
    public static event Action<GameStateLeftMessage> KerbalSpaceCenterStateLeft;
    public static event Action<GameStateLeftMessage> VehicleAssemblyBuilderLeft;
    public static event Action<GameStateLeftMessage> BaseAssemblyEditorLeft;
    public static event Action<GameStateLeftMessage> FlightViewLeft;
    public static event Action<GameStateLeftMessage> ColonyViewLeft;
    public static event Action<GameStateLeftMessage> PhotoModeLeft;
    public static event Action<GameStateLeftMessage> Map3DViewLeft;
    public static event Action<GameStateLeftMessage> MetricsModeLeft;
    public static event Action<GameStateLeftMessage> PlanetViewerLeft;
    public static event Action<GameStateLeftMessage> LoadingLeft;
    public static event Action<GameStateLeftMessage> TrainingCenterLeft;
    public static event Action<GameStateLeftMessage> MissionControlLeft;
    public static event Action<GameStateLeftMessage> TrackingStationLeft;
    public static event Action<GameStateLeftMessage> ResearchAndDevelopmentLeft;
    public static event Action<GameStateLeftMessage> LaunchpadLeft;
    public static event Action<GameStateLeftMessage> RunwayLeft;
    public static event Action<GameStateLeftMessage> FlagLeft;

    #endregion

    #region State Handling

    internal static void OnGameStateEntered(MessageCenterMessage message)
    {
        var msg = message as GameStateEnteredMessage;
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
        var msg = message as GameStateLeftMessage;
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

    internal static void OnGameStateChanged(MessageCenterMessage message)
    {
        var msg = message as GameStateChangedMessage;
        GameStateChanged?.Invoke(msg!, msg!.PreviousState, msg!.CurrentState);
    }
    #endregion
}