using JetBrains.Annotations;
using KSP.Game;
using KSP.Messages;

namespace SpaceWarp.API.Game.Messages;

/// <summary>
/// A class that contains a list of events that are published either when a state is entered or left
/// </summary>
[PublicAPI]
public static class StateChanges
{
    #region State changing

    /// <summary>
    /// Invoked when the game state is changed
    /// <para>Action(Message,PreviousState,CurrentState)</para>
    /// </summary>
    public static event Action<GameStateChangedMessage, GameState, GameState> GameStateChanged;

    #endregion

    #region Entering States

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.Invalid"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> InvalidStateEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.WarmUpLoading"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> WarmUpLoadingStateEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.MainMenu"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> MainMenuStateEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.KerbalSpaceCenter"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> KerbalSpaceCenterStateEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.VehicleAssemblyBuilder"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> VehicleAssemblyBuilderEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.BaseAssemblyEditor"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> BaseAssemblyEditorEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.FlightView"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> FlightViewEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.ColonyView"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> ColonyViewEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.Map3DView"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> Map3DViewEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.PhotoMode"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> PhotoModeEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.MetricsMode"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> MetricsModeEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.PlanetViewer"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> PlanetViewerEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.Loading"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> LoadingEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.TrainingCenter"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> TrainingCenterEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.MissionControl"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> MissionControlEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.TrackingStation"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> TrackingStationEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.ResearchAndDevelopment"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> ResearchAndDevelopmentEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.Launchpad"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> LaunchpadEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.Runway"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> RunwayEntered;

    /// <summary>
    /// Invoked when the game state is changed to <see cref="GameState.Flag"/>
    /// </summary>
    public static event Action<GameStateEnteredMessage> FlagEntered;

    #endregion

    #region Leaving States

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.Invalid"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> InvalidStateLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.WarmUpLoading"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> WarmUpLoadingStateLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.MainMenu"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> MainMenuStateLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.KerbalSpaceCenter"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> KerbalSpaceCenterStateLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.VehicleAssemblyBuilder"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> VehicleAssemblyBuilderLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.BaseAssemblyEditor"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> BaseAssemblyEditorLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.FlightView"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> FlightViewLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.ColonyView"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> ColonyViewLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.Map3DView"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> PhotoModeLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.PhotoMode"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> Map3DViewLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.MetricsMode"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> MetricsModeLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.PlanetViewer"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> PlanetViewerLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.Loading"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> LoadingLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.TrainingCenter"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> TrainingCenterLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.MissionControl"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> MissionControlLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.TrackingStation"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> TrackingStationLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.ResearchAndDevelopment"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> ResearchAndDevelopmentLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.Launchpad"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> LaunchpadLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.Runway"/>
    /// </summary>
    public static event Action<GameStateLeftMessage> RunwayLeft;

    /// <summary>
    /// Invoked when the game state is changed from <see cref="GameState.Flag"/>
    /// </summary>
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