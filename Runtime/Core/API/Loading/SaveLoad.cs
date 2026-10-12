using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using ReduxLib.GameInterfaces;
using SpaceWarp2.API.Features;
using SpaceWarp2.API.Mods;
using UnityEngine;

namespace SpaceWarp2.API.Loading;

/// <summary>
/// An API to register flow actions to be run during loading and saving.
/// </summary>
[PublicAPI]
public static class SaveLoad
{
    public static Dictionary<string, List<IFlowAction>> GameInitializingFlowActions = new();
    public static Dictionary<string, List<IFlowAction>> LoadingFlowActions = new();
    public static Dictionary<string, List<Type>> LoadingFlowActionTypes = new();
    public static Dictionary<string, List<IFlowAction>> SavingFlowActions = new();
    public static Dictionary<string, List<Type>> SavingFlowActionTypes = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        GameInitializingFlowActions = new();
        LoadingFlowActions = new();
        LoadingFlowActionTypes = new();
        SavingFlowActions = new();
        SavingFlowActionTypes = new();
        // A play session that ended without unloading leaves nothing for the next one to tear down
        CampaignActions.Clear();
        OwedUnloads.Clear();
        AnchoredOwners.Clear();
    }

    // The mod each anchored action or action type was registered for, so it runs only while that mod is enabled
    private static readonly Dictionary<object, string> AnchoredOwners = new();

    // Work mods do every time a campaign loads, by where in loading it runs
    private static readonly Dictionary<CampaignPhase, List<FeatureGatedAction>> CampaignActions = new();

    // Teardown owed for the campaign loaded now, run latest first when it unloads
    private static readonly List<Action> OwedUnloads = new();

    /// <summary>
    /// Registers work a mod does every time a campaign it is enabled in loads, a quickload or revert into the same
    /// set included.
    /// </summary>
    /// <param name="modId">The mod the work belongs to.</param>
    /// <param name="name">The name shown while it runs.</param>
    /// <param name="phase">Where in loading it runs.</param>
    /// <param name="load">The work, calling its argument once done. Teardown for what it sets up is registered
    /// with <see cref="OnCampaignUnload" />.</param>
    /// <returns>The action, for chaining feature selectors.</returns>
    public static FeatureGatedAction AddCampaignLoadAction(
        string modId,
        string name,
        CampaignPhase phase,
        Action<Action> load
    )
    {
        var action = new FeatureGatedAction(modId, name, load);
        if (!CampaignActions.TryGetValue(phase, out List<FeatureGatedAction> actions))
        {
            CampaignActions[phase] = actions = new List<FeatureGatedAction>();
        }

        actions.Add(action);
        return action;
    }

    /// <summary>
    /// Registers something a mod sets up every time a campaign it is enabled in loads, and disposes when the
    /// campaign unloads.
    /// </summary>
    /// <param name="modId">The mod the scope belongs to.</param>
    /// <param name="name">The name shown while it is set up.</param>
    /// <param name="phase">Where in loading it is set up.</param>
    /// <param name="open">Sets the scope up and returns what disposing tears it down.</param>
    /// <returns>The action, for chaining feature selectors.</returns>
    public static FeatureGatedAction AddCampaignScope(
        string modId,
        string name,
        CampaignPhase phase,
        Func<IDisposable> open
    ) => AddCampaignLoadAction(modId, name, phase, done =>
    {
        IDisposable scope = open();
        if (scope != null)
        {
            OnCampaignUnload(scope.Dispose);
        }

        done();
    });

    /// <summary>
    /// Registers teardown for something set up for the campaign loaded now, run when the campaign unloads.
    /// </summary>
    /// <remarks>
    /// Unloads run in the reverse order they were registered, on quitting to the main menu, before any other load
    /// replaces the campaign, and when a load fails part way.
    /// </remarks>
    /// <param name="unload">The teardown.</param>
    public static void OnCampaignUnload(Action unload)
    {
        OwedUnloads.Add(unload);
    }

    /// <summary>
    /// Gets the work registered for one phase of loading a campaign, in registration order.
    /// </summary>
    /// <param name="phase">The phase.</param>
    /// <returns>The actions.</returns>
    public static IReadOnlyList<FeatureGatedAction> CampaignActionsIn(CampaignPhase phase) =>
        CampaignActions.TryGetValue(phase, out List<FeatureGatedAction> actions)
            ? actions
            : Array.Empty<FeatureGatedAction>();

    /// <summary>
    /// Lists the steps that run campaign load work.
    /// </summary>
    /// <remarks>
    /// Each step decides whether its action applies when it runs, which is after the campaign's set activated.
    /// </remarks>
    /// <param name="actions">The actions, normally <see cref="CampaignActionsIn" /> for one phase.</param>
    /// <returns>Each step's name and work, the work given a callback to call once done.</returns>
    public static List<(string name, Action<Action> work)> CampaignSteps(IEnumerable<FeatureGatedAction> actions)
    {
        var steps = new List<(string name, Action<Action> work)>();
        foreach (FeatureGatedAction action in actions)
        {
            steps.Add((action.Name, done =>
            {
                if (!action.Applies())
                {
                    done();
                    return;
                }

                try
                {
                    action.Run(done);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SpaceWarp] Campaign load action {action.Name} of {action.ModId} failed: {e}");
                    done();
                }
            }));
        }

        return steps;
    }

    /// <summary>
    /// Tells each enabled plugin that the campaign has loaded, and owes it the matching unload.
    /// </summary>
    /// <param name="plugins">Each plugin with its mod id.</param>
    public static void NotifyCampaignLoaded(IEnumerable<(string modId, ICampaignLifecycle plugin)> plugins)
    {
        foreach ((string modId, ICampaignLifecycle plugin) in plugins)
        {
            if (!ActiveFeatures.IsModEnabled(modId))
                continue;

            try
            {
                plugin.OnCampaignLoaded();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SpaceWarp] {modId} failed to load its campaign state: {e}");
            }

            OwedUnloads.Add(plugin.OnCampaignUnloading);
        }
    }

    /// <summary>
    /// Runs every unload owed for the campaign loaded now, latest first, and forgets them.
    /// </summary>
    public static void UnloadCampaign()
    {
        // Taken out first, so an unload that triggers another unload finds nothing left to run twice
        var owed = new List<Action>(OwedUnloads);
        OwedUnloads.Clear();
        for (int i = owed.Count - 1; i >= 0; i--)
        {
            try
            {
                owed[i]();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SpaceWarp] A campaign unload failed: {e}");
            }
        }
    }

    /// <summary>
    ///     <para>Add a <cref>FlowAction</cref> to the Game's load sequence.</para>
    ///
    ///     <para>The action will be run after the first FlowAction with a name equal to referenceAction.
    ///     If referenceAction is <c>null</c>, the action will be the first action run.</para>
    ///
    ///     <para>
    ///         The FlowActions that occur in this sequence by default are: (Updated for KSP 0.1.1.0)
    ///         <list type="number">
    ///             <item><c>"Creating Splash Screens Prefab"</c></item>
    ///             <item><c>"Set Loading Optimizations"</c></item>
    ///             <item><c>"Initialize Addressables system"</c></item>
    ///             <item><c>"Creating Game Instance"</c></item>
    ///             <item><c>"Initializing Game Instance"</c></item>
    ///             <item><c>"Creating Graphics Manager"</c></item>
    ///             <item><c>"Creating Main Menu"</c></item>
    ///             <item><c>"Load UI Background Scenes"</c></item>
    ///             <item><c>"Parsing Loading Screens"</c></item>
    ///             <item><c>"Set Loading Optimizations"</c></item>
    ///             <item><c>"loading addressable localizations"</c> (Added by SpaceWarp)</item>
    ///         </list>
    ///     </para>
    /// </summary>
    /// <param name="flowAction">The FlowAction to insert</param>
    /// <param name="referenceAction">The name of the action to insert a TFlowActionType after. Use <c>null</c> to insert it at the start.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AddFlowActionToGameLoadAfter(IFlowAction flowAction, string referenceAction)
    {
        AddFlowActionToGameLoadAfter(PluginList.ModIdOf(Assembly.GetCallingAssembly()), flowAction, referenceAction);
    }

    /// <summary>
    ///     <para>Add a <cref>FlowAction</cref> to the save file loading sequence. A new <c>TFlowActionType</c> is constructed every load.</para>
    ///
    ///     <para>
    ///         TFlowActionType must have a public constructor that at most one of each of the following types:
    ///         <list type="bullet">
    ///             <item><cref>SaveLoadManager</cref></item>
    ///             <item><cref>LoadOrSaveCampaignTicket</cref></item>
    ///             <item><cref>LoadGameData</cref></item>
    ///         </list>
    ///     </para>
    ///
    ///     <para>The action will be run after the first FlowAction with a name equal to referenceAction.
    ///     If referenceAction is <c>null</c>, the action will be the first action run.</para>
    ///
    ///     <para>
    ///         The FlowActions that occur in this sequence by default are: (Updated for KSP 0.1.1.0)
    ///         <list type="number">
    ///             <item><c>"Deserializing Save File Contents"</c></item>
    ///             <item><c>"Starting..."</c></item>
    ///             <item><c>"Loading session manager data"</c></item>
    ///             <item><c>"Set Loading Optimizations"</c></item>
    ///             <item><c>"Setup local player"</c></item>
    ///             <item><c>"Load Session Guid"</c></item>
    ///             <item><c>"Loading KSP2 mission base"</c></item>
    ///             <item><c>"Load Campaign Players"</c></item>
    ///             <item><c>"Load Agencies"</c></item>
    ///             <item><c>"Load Fixup Player And Agency"</c></item>
    ///             <item><c>"Setup OAB Part Seed Counter"</c></item>
    ///             <item><c>"Unload Main Menu"</c></item>
    ///             <item><c>"Validating Version Number"</c></item>
    ///             <item><c>"Initializing Session Data"</c></item>
    ///             <item><c>"Load required assets"</c></item>
    ///             <item><c>"Parsing resource assets"</c></item>
    ///             <item><c>"Freezing resource definition database"</c></item>
    ///             <item><c>"Parsing procedural part assets"</c></item>
    ///             <item><c>"Freezing procedural part definition database"</c></item>
    ///             <item><c>"Loading Kerbal Prefab Data"</c></item>
    ///             <item><c>"Loading Kerbal Roster Data"</c></item>
    ///             <item><c>"Loading Colony Data"</c></item>
    ///             <item><c>"Setup Properties"</c></item>
    ///             <item><c>"Creating Simulation"</c></item>
    ///             <item><c>"Loading Celestial Body Data"</c></item>
    ///             <item><c>"Parsing interstellar universe"</c></item>
    ///             <item><c>"Parsing parts text assets"</c></item>
    ///             <item><c>"Creating Celestial Bodies"</c></item>
    ///             <item><c>"Pumping Sim Once"</c></item>
    ///             <item><c>"Creating Universe"</c></item>
    ///             <item><c>"Creating ViewController"</c></item>
    ///             <item><c>"Pumping Sim Once"</c></item>
    ///             <item><c>"Spawning initial local-space body..."</c></item>
    ///             <item><c>"Loading Map Systems"</c></item>
    ///             <item><c>"Loading UI Manager"</c></item>
    ///             <item><c>"Loading KSP2 missions"</c></item>
    ///             <item><c>"Setting Universe State"</c></item>
    ///             <item><c>"Creating Vessel"</c></item>
    ///             <item><c>"Set Active Vessel for Local Player"</c></item>
    ///             <item><c>"Loading Travel Log Data"</c></item>
    ///             <item><c>"Pumping Sim Once"</c></item>
    ///             <item><c>"Loading dashboard for the user"</c></item>
    ///             <item><c>"Setting Camera State"</c></item>
    ///             <item><c>"Starting Sim"</c></item>
    ///             <item><c>"Pumping Sim Once"</c></item>
    ///             <item><c>"Applying flight control state"</c></item>
    ///             <item><c>"Applying legacy module data to vessel parts"</c></item>
    ///             <item><c>"Load Planted Flags"</c></item>
    ///             <item><c>"Wait for async handles"</c></item>
    ///             <item><c>"Load Done"</c></item>
    ///             <item><c>"Set Loading Optimizations"</c></item>
    ///             <item><c>"Ending..."</c></item>
    ///         </list>
    ///     </para>
    /// </summary>
    /// <typeparam name="TFlowActionType">The type of FlowAction to insert</typeparam>
    /// <param name="referenceAction">The name of the action to insert a TFlowActionType after. Use <c>null</c> to insert it at the start.</param>
    /// <exception cref="InvalidOperationException">Thrown if <c>TFlowActionType</c> does not have a valid Constructor</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AddFlowActionToCampaignLoadAfter<TFlowActionType>(string referenceAction) where TFlowActionType : IFlowAction
    {
        AddFlowActionToCampaignLoadAfter<TFlowActionType>(PluginList.ModIdOf(Assembly.GetCallingAssembly()), referenceAction);
    }

    /// <summary>
    ///     <para>Add a <cref>FlowAction</cref> to the save file loading sequence. The same object is used for every load.</para>
    ///
    ///     <para>The action will be run after the first FlowAction with a name equal to referenceAction.
    ///     If referenceAction is <c>null</c>, the action will be the first action run.</para>
    ///
    ///     <para>
    ///         The FlowActions that occur in this sequence by default are: (Updated for KSP 0.1.1.0)
    ///         <list type="number">
    ///             <item><c>"Deserializing Save File Contents"</c></item>
    ///             <item><c>"Starting..."</c></item>
    ///             <item><c>"Loading session manager data"</c></item>
    ///             <item><c>"Set Loading Optimizations"</c></item>
    ///             <item><c>"Setup local player"</c></item>
    ///             <item><c>"Load Session Guid"</c></item>
    ///             <item><c>"Loading KSP2 mission base"</c></item>
    ///             <item><c>"Load Campaign Players"</c></item>
    ///             <item><c>"Load Agencies"</c></item>
    ///             <item><c>"Load Fixup Player And Agency"</c></item>
    ///             <item><c>"Setup OAB Part Seed Counter"</c></item>
    ///             <item><c>"Unload Main Menu"</c></item>
    ///             <item><c>"Validating Version Number"</c></item>
    ///             <item><c>"Initializing Session Data"</c></item>
    ///             <item><c>"Load required assets"</c></item>
    ///             <item><c>"Parsing resource assets"</c></item>
    ///             <item><c>"Freezing resource definition database"</c></item>
    ///             <item><c>"Parsing procedural part assets"</c></item>
    ///             <item><c>"Freezing procedural part definition database"</c></item>
    ///             <item><c>"Loading Kerbal Prefab Data"</c></item>
    ///             <item><c>"Loading Kerbal Roster Data"</c></item>
    ///             <item><c>"Loading Colony Data"</c></item>
    ///             <item><c>"Setup Properties"</c></item>
    ///             <item><c>"Creating Simulation"</c></item>
    ///             <item><c>"Loading Celestial Body Data"</c></item>
    ///             <item><c>"Parsing interstellar universe"</c></item>
    ///             <item><c>"Parsing parts text assets"</c></item>
    ///             <item><c>"Creating Celestial Bodies"</c></item>
    ///             <item><c>"Pumping Sim Once"</c></item>
    ///             <item><c>"Creating Universe"</c></item>
    ///             <item><c>"Creating ViewController"</c></item>
    ///             <item><c>"Pumping Sim Once"</c></item>
    ///             <item><c>"Spawning initial local-space body..."</c></item>
    ///             <item><c>"Loading Map Systems"</c></item>
    ///             <item><c>"Loading UI Manager"</c></item>
    ///             <item><c>"Loading KSP2 missions"</c></item>
    ///             <item><c>"Setting Universe State"</c></item>
    ///             <item><c>"Creating Vessel"</c></item>
    ///             <item><c>"Set Active Vessel for Local Player"</c></item>
    ///             <item><c>"Loading Travel Log Data"</c></item>
    ///             <item><c>"Pumping Sim Once"</c></item>
    ///             <item><c>"Loading dashboard for the user"</c></item>
    ///             <item><c>"Setting Camera State"</c></item>
    ///             <item><c>"Starting Sim"</c></item>
    ///             <item><c>"Pumping Sim Once"</c></item>
    ///             <item><c>"Applying flight control state"</c></item>
    ///             <item><c>"Applying legacy module data to vessel parts"</c></item>
    ///             <item><c>"Load Planted Flags"</c></item>
    ///             <item><c>"Wait for async handles"</c></item>
    ///             <item><c>"Load Done"</c></item>
    ///             <item><c>"Set Loading Optimizations"</c></item>
    ///             <item><c>"Ending..."</c></item>
    ///         </list>
    ///     </para>
    /// </summary>
    /// <param name="flowAction">The FlowAction to insert</param>
    /// <param name="referenceAction">The name of the action to insert a FlowActionType after. Use <c>null</c> to insert it at the start.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AddFlowActionToCampaignLoadAfter(IFlowAction flowAction, string referenceAction)
    {
        AddFlowActionToCampaignLoadAfter(PluginList.ModIdOf(Assembly.GetCallingAssembly()), flowAction, referenceAction);
    }

    /// <summary>
    ///     <para>Add a <cref>FlowAction</cref> to the save file writing sequence. A new <c>TFlowActionType</c> is constructed every load.</para>
    ///
    ///     <para>
    ///         TFlowActionType must have a public constructor that at most one of each of the following types:
    ///         <list type="bullet">
    ///             <item><cref>SaveLoadManager</cref></item>
    ///             <item><cref>LoadOrSaveCampaignTicket</cref></item>
    ///         </list>
    ///     </para>
    ///
    ///     <para>The action will be run after the first FlowAction with a name equal to referenceAction.
    ///     If referenceAction is <c>null</c>, the action will be the first action run.</para>
    ///
    ///     <para>
    ///         The FlowActions that occur in this sequence by default are: (Updated for KSP 0.1.1.0)
    ///         <list type="number">
    ///             <item><c>"Collecting vessel data for serialization"</c></item>
    ///             <item><c>"Saving session manager"</c></item>
    ///             <item><c>"Setting version"</c></item>
    ///             <item><c>"Saving session guid"</c></item>
    ///             <item><c>"Collecting game session metadata"</c></item>
    ///             <item><c>"Saving Kerbal roster"</c></item>
    ///             <item><c>"Saving colony data"</c></item>
    ///             <item><c>"Saving Travel Log"</c></item>
    ///             <item><c>"Collecting mission data for serialization"</c></item>
    ///             <item><c>"Collecting OAB workspace data for serialization"</c></item>
    ///             <item><c>"Saving planted flags"</c></item>
    ///             <item><c>"Saving agencies"</c></item>
    ///             <item><c>"Saving campaign players"</c></item>
    ///             <item><c>"Serializing Save File Contents"</c></item>
    ///         </list>
    ///     </para>
    /// </summary>
    /// <typeparam name="TFlowActionType">The type of FlowAction to insert</typeparam>
    /// <param name="referenceAction">The name of the action to insert a TFlowActionType after. Use <c>null</c> to insert it at the start.</param>
    /// <exception cref="InvalidOperationException">Thrown if <c>TFlowActionType</c> does not have a valid Constructor</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AddFlowActionToCampaignSaveAfter<TFlowActionType>(string referenceAction) where TFlowActionType : IFlowAction
    {
        AddFlowActionToCampaignSaveAfter<TFlowActionType>(PluginList.ModIdOf(Assembly.GetCallingAssembly()), referenceAction);
    }

    /// <summary>
    ///     <para>Add a <cref>FlowAction</cref> to the save file writing sequence. The same object is used for every load.</para>
    ///
    ///     <para>The action will be run after the first FlowAction with a name equal to referenceAction.
    ///     If referenceAction is <c>null</c>, the action will be the first action run.</para>
    ///
    ///     <para>
    ///         The FlowActions that occur in this sequence by default are: (Updated for KSP 0.1.1.0)
    ///         <list type="number">
    ///             <item><c>"Collecting vessel data for serialization"</c></item>
    ///             <item><c>"Saving session manager"</c></item>
    ///             <item><c>"Setting version"</c></item>
    ///             <item><c>"Saving session guid"</c></item>
    ///             <item><c>"Collecting game session metadata"</c></item>
    ///             <item><c>"Saving Kerbal roster"</c></item>
    ///             <item><c>"Saving colony data"</c></item>
    ///             <item><c>"Saving Travel Log"</c></item>
    ///             <item><c>"Collecting mission data for serialization"</c></item>
    ///             <item><c>"Collecting OAB workspace data for serialization"</c></item>
    ///             <item><c>"Saving planted flags"</c></item>
    ///             <item><c>"Saving agencies"</c></item>
    ///             <item><c>"Saving campaign players"</c></item>
    ///             <item><c>"Serializing Save File Contents"</c></item>
    ///         </list>
    ///     </para>
    /// </summary>
    /// <param name="flowAction">The FlowAction to insert</param>
    /// <param name="referenceAction">The name of the action to insert a FlowActionType after. Use <c>null</c> to insert it at the start.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AddFlowActionToCampaignSaveAfter(IFlowAction flowAction, string referenceAction)
    {
        AddFlowActionToCampaignSaveAfter(PluginList.ModIdOf(Assembly.GetCallingAssembly()), flowAction, referenceAction);
    }

    /// <summary>
    /// Adds a flow action to the game's load sequence, after the first action with the given name, owned by a mod.
    /// </summary>
    /// <param name="modId">The mod the action belongs to, or null for an action that always runs.</param>
    /// <param name="flowAction">The FlowAction to insert.</param>
    /// <param name="referenceAction">The name of the action to insert it after.</param>
    public static void AddFlowActionToGameLoadAfter(string modId, IFlowAction flowAction, string referenceAction)
    {
        Anchor(GameInitializingFlowActions, referenceAction, flowAction, modId);
    }

    /// <summary>
    /// Adds a flow action type to the save file loading sequence that runs only while its mod is enabled in the
    /// loaded campaign's feature set. A new <c>TFlowActionType</c> is constructed every load.
    /// </summary>
    /// <typeparam name="TFlowActionType">The type of FlowAction to insert.</typeparam>
    /// <param name="modId">The mod the action belongs to, or null for an action that always runs.</param>
    /// <param name="referenceAction">The name of the action to insert it after.</param>
    public static void AddFlowActionToCampaignLoadAfter<TFlowActionType>(string modId, string referenceAction)
        where TFlowActionType : IFlowAction
    {
        Anchor(LoadingFlowActionTypes, referenceAction, typeof(TFlowActionType), modId);
    }

    /// <summary>
    /// Adds a flow action to the save file loading sequence that runs only while its mod is enabled in the loaded
    /// campaign's feature set. The same object is used for every load.
    /// </summary>
    /// <param name="modId">The mod the action belongs to, or null for an action that always runs.</param>
    /// <param name="flowAction">The FlowAction to insert.</param>
    /// <param name="referenceAction">The name of the action to insert it after.</param>
    public static void AddFlowActionToCampaignLoadAfter(string modId, IFlowAction flowAction, string referenceAction)
    {
        Anchor(LoadingFlowActions, referenceAction, flowAction, modId);
    }

    /// <summary>
    /// Adds a flow action type to the save file saving sequence that runs only while its mod is enabled in the
    /// campaign's feature set. A new <c>TFlowActionType</c> is constructed every save.
    /// </summary>
    /// <typeparam name="TFlowActionType">The type of FlowAction to insert.</typeparam>
    /// <param name="modId">The mod the action belongs to, or null for an action that always runs.</param>
    /// <param name="referenceAction">The name of the action to insert it after.</param>
    public static void AddFlowActionToCampaignSaveAfter<TFlowActionType>(string modId, string referenceAction)
        where TFlowActionType : IFlowAction
    {
        Anchor(SavingFlowActionTypes, referenceAction, typeof(TFlowActionType), modId);
    }

    /// <summary>
    /// Adds a flow action to the save file saving sequence that runs only while its mod is enabled in the campaign's
    /// feature set. The same object is used for every save.
    /// </summary>
    /// <param name="modId">The mod the action belongs to, or null for an action that always runs.</param>
    /// <param name="flowAction">The FlowAction to insert.</param>
    /// <param name="referenceAction">The name of the action to insert it after.</param>
    public static void AddFlowActionToCampaignSaveAfter(string modId, IFlowAction flowAction, string referenceAction)
    {
        Anchor(SavingFlowActions, referenceAction, flowAction, modId);
    }

    /// <summary>
    /// Gets the mod an anchored flow action or flow action type was registered for.
    /// </summary>
    /// <param name="actionOrType">The registered action, or the registered action type.</param>
    /// <returns>The mod's id, or null for an action that always runs.</returns>
    public static string OwnerOf(object actionOrType) => AnchoredOwners.GetValueOrDefault(actionOrType);

    private static void Anchor<T>(Dictionary<string, List<T>> anchors, string referenceAction, T entry, string modId)
    {
        if (anchors.TryGetValue(referenceAction, out List<T> entries))
        {
            entries.Add(entry);
        }
        else
        {
            anchors[referenceAction] = new List<T> { entry };
        }

        if (modId != null)
        {
            AnchoredOwners[entry] = modId;
        }
    }
}

/// <summary>
/// Where in loading a campaign the work registered with <see cref="SaveLoad.AddCampaignLoadAction" /> runs.
/// </summary>
[PublicAPI]
public enum CampaignPhase
{
    /// <summary>
    /// Right after the campaign's feature set activates, before any of its content loads.
    /// </summary>
    Content,

    /// <summary>
    /// After the parts are read, before the universe is created.
    /// </summary>
    BeforeUniverse,

    /// <summary>
    /// Once the campaign has finished loading.
    /// </summary>
    Loaded
}

/// <summary>
/// Implemented by a mod's plugin to hear when a campaign it is enabled in loads and unloads.
/// </summary>
[PublicAPI]
public interface ICampaignLifecycle
{
    /// <summary>
    /// Called once a campaign the mod is enabled in has loaded.
    /// </summary>
    void OnCampaignLoaded();

    /// <summary>
    /// Called before that campaign unloads, to undo what <see cref="OnCampaignLoaded" /> set up.
    /// </summary>
    void OnCampaignUnloading();
}
