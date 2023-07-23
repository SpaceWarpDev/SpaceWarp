using System;
using KSP.Game.Flow;
using SpaceWarp.Patching;

namespace SpaceWarp.API.Loading;

public static class SaveLoad
{
    /// <summary>
    ///     <para>Construct and add a <cref>FlowAction</cref> to the Game's load sequence.</para>
    ///     
    ///     <para>FlowActionType must have a public constructor that takes either no arguments,
    ///     or a single GameManager.</para>
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
    /// <typeparam name="FlowActionType">The type of FlowAction to insert</typeparam>
    /// <param name="referenceAction">The name of the action to insert a FlowActionType after. Use <c>null</c> to insert it at the start.</param>
    /// <exception cref="InvalidOperationException">Thrown if <c>FlowActionType</c> does not have a valid Constructor</exception>
    public static void AddFlowActionToGameLoadAfter<FlowActionType>(string referenceAction) where FlowActionType : FlowAction
    {
        SequentialFlowLoadersPatcher.AddConstructor(referenceAction, typeof(FlowActionType), SequentialFlowLoadersPatcher.FLOW_METHOD_STARTGAME);
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
    /// <param name="referenceAction">The name of the action to insert a FlowActionType after. Use <c>null</c> to insert it at the start.</param>
    public static void AddFlowActionToGameLoadAfter(FlowAction flowAction, string referenceAction)
    {
        SequentialFlowLoadersPatcher.AddFlowAction(referenceAction, flowAction, SequentialFlowLoadersPatcher.FLOW_METHOD_STARTGAME);
    }

    /// <summary>
    ///     <para>Add a <cref>FlowAction</cref> to the save file loading sequence. A new <c>FlowActionType</c> is constructed every load.</para>
    ///     
    ///     <para>
    ///         FlowActionType must have a public constructor that at most one of each of the following types:
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
    /// <typeparam name="FlowActionType">The type of FlowAction to insert</typeparam>
    /// <param name="referenceAction">The name of the action to insert a FlowActionType after. Use <c>null</c> to insert it at the start.</param>
    /// <exception cref="InvalidOperationException">Thrown if <c>FlowActionType</c> does not have a valid Constructor</exception>
    public static void AddFlowActionToCampaignLoadAfter<FlowActionType>(string referenceAction) where FlowActionType : FlowAction
    {
        SequentialFlowLoadersPatcher.AddConstructor(referenceAction, typeof(FlowActionType), SequentialFlowLoadersPatcher.FLOW_METHOD_PRIVATELOADCOMMON);
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
    public static void AddFlowActionToCampaignLoadAfter(FlowAction flowAction, string referenceAction)
    {
        SequentialFlowLoadersPatcher.AddFlowAction(referenceAction, flowAction, SequentialFlowLoadersPatcher.FLOW_METHOD_PRIVATELOADCOMMON);
    }

    /// <summary>
    ///     <para>Add a <cref>FlowAction</cref> to the save file writing sequence. A new <c>FlowActionType</c> is constructed every load.</para>
    ///     
    ///     <para>
    ///         FlowActionType must have a public constructor that at most one of each of the following types:
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
    /// <typeparam name="FlowActionType">The type of FlowAction to insert</typeparam>
    /// <param name="referenceAction">The name of the action to insert a FlowActionType after. Use <c>null</c> to insert it at the start.</param>
    /// <exception cref="InvalidOperationException">Thrown if <c>FlowActionType</c> does not have a valid Constructor</exception>
    public static void AddFlowActionToCampaignSaveAfter<FlowActionType>(string referenceAction) where FlowActionType : FlowAction
    {
        SequentialFlowLoadersPatcher.AddConstructor(referenceAction, typeof(FlowActionType), SequentialFlowLoadersPatcher.FLOW_METHOD_PRIVATESAVECOMMON);
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
    public static void AddFlowActionToCampaignSaveAfter(FlowAction flowAction, string referenceAction)
    {
        SequentialFlowLoadersPatcher.AddFlowAction(referenceAction, flowAction, SequentialFlowLoadersPatcher.FLOW_METHOD_PRIVATESAVECOMMON);
    }
}
