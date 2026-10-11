using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SpaceWarp2.API.Features;
using SpaceWarp2.API.Mods;
using SpaceWarp2.UI.Backend.UI.Appbar;
using UnityEngine;

namespace SpaceWarp2.UI.API.Appbar;

[PublicAPI]
public static class Appbar
{
    private static readonly List<(string text, Sprite icon, string ID, Action<bool> action, string owner)> ButtonsToBeLoaded = new();

    private static readonly List<(string text, Sprite icon, string ID, Action<bool> action, string owner)> OabButtonsToBeLoaded = new();

    private static readonly List<(string text, Sprite icon, string ID, Action action, string owner)> KscButtonsToBeLoaded = new();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterAppButton(string text, string id, Sprite icon, Action<bool> func) =>
        RegisterAppButton(OwnerOf(Assembly.GetCallingAssembly()), text, id, icon, func);

    /// <summary>
    /// Registers a flight app bar button that shows only while its mod is enabled in the active feature set.
    /// </summary>
    /// <param name="modId">The mod the button belongs to.</param>
    /// <param name="text">The button's text.</param>
    /// <param name="id">The button's id, which a later registration with the same id replaces.</param>
    /// <param name="icon">The button's icon.</param>
    /// <param name="func">Called with the button's new toggle state.</param>
    public static void RegisterAppButton(string modId, string text, string id, Sprite icon, Action<bool> func)
    {
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"RegisterAppButton: {text}, {id}");
        // Upsert by id: registration re-runs on every Play Mode enter and this static list persists
        // across sessions when Domain Reload is disabled, so a plain Add would create duplicate appbar
        // buttons. Replacing the same-id entry also keeps the callback fresh (last registration wins).
        ButtonsToBeLoaded.RemoveAll(b => b.ID == id);
        ButtonsToBeLoaded.Add((text, icon, id, func, modId));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterAppButton(string text, string id, Texture2D icon, Action<bool> func) =>
        RegisterAppButton(OwnerOf(Assembly.GetCallingAssembly()), text, id, GetAppBarIconFromTexture(icon), func);

    // ReSharper disable once InconsistentNaming
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterOABAppButton(string text, string id, Sprite icon, Action<bool> func) =>
        RegisterOABAppButton(OwnerOf(Assembly.GetCallingAssembly()), text, id, icon, func);

    /// <summary>
    /// Registers an assembly building app bar button that shows only while its mod is enabled in the active
    /// feature set.
    /// </summary>
    /// <param name="modId">The mod the button belongs to.</param>
    /// <param name="text">The button's text.</param>
    /// <param name="id">The button's id, which a later registration with the same id replaces.</param>
    /// <param name="icon">The button's icon.</param>
    /// <param name="func">Called with the button's new toggle state.</param>
    // ReSharper disable once InconsistentNaming
    public static void RegisterOABAppButton(string modId, string text, string id, Sprite icon, Action<bool> func)
    {
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"RegisterOABAppButton: {text}, {id}");
        // Upsert by id. See RegisterAppButton (avoids duplicate buttons across Play Mode sessions).
        OabButtonsToBeLoaded.RemoveAll(b => b.ID == id);
        OabButtonsToBeLoaded.Add((text, icon, id, func, modId));
    }

    // ReSharper disable once InconsistentNaming
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterOABAppButton(string text, string id, Texture2D icon, Action<bool> func) =>
        RegisterOABAppButton(OwnerOf(Assembly.GetCallingAssembly()), text, id, GetAppBarIconFromTexture(icon), func);

    // ReSharper disable once InconsistentNaming
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterKSCAppButton(string text, string id, Sprite icon, Action func) =>
        RegisterKSCAppButton(OwnerOf(Assembly.GetCallingAssembly()), text, id, icon, func);

    /// <summary>
    /// Registers a space center app bar button that shows only while its mod is enabled in the active feature
    /// set.
    /// </summary>
    /// <param name="modId">The mod the button belongs to.</param>
    /// <param name="text">The button's text.</param>
    /// <param name="id">The button's id, which a later registration with the same id replaces.</param>
    /// <param name="icon">The button's icon.</param>
    /// <param name="func">Called when the button is clicked.</param>
    // ReSharper disable once InconsistentNaming
    public static void RegisterKSCAppButton(string modId, string text, string id, Sprite icon, Action func)
    {
        // Upsert by id. See RegisterAppButton (avoids duplicate buttons across Play Mode sessions).
        KscButtonsToBeLoaded.RemoveAll(b => b.ID == id);
        KscButtonsToBeLoaded.Add((text, icon, id, func, modId));
    }

    // ReSharper disable once InconsistentNaming
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterKSCAppButton(string text, string id, Texture2D icon, Action func) =>
        RegisterKSCAppButton(OwnerOf(Assembly.GetCallingAssembly()), text, id, GetAppBarIconFromTexture(icon), func);

    /// <summary>
    /// Removes a button from every app bar, for a button registered for one campaign only.
    /// </summary>
    /// <param name="id">The button's id.</param>
    public static void Unregister(string id)
    {
        ButtonsToBeLoaded.RemoveAll(b => b.ID == id);
        OabButtonsToBeLoaded.RemoveAll(b => b.ID == id);
        KscButtonsToBeLoaded.RemoveAll(b => b.ID == id);
        IAppbarBackend.Instance?.RemoveButton(id);
    }

    /// <summary>
    /// Checks whether a button its owner registered shows in the active feature set.
    /// </summary>
    /// <param name="ownerModId">The mod that registered the button, or null when no mod could be told.</param>
    /// <returns>True if the button shows.</returns>
    public static bool ShowsButtonOf(string ownerModId) =>
        ownerModId == null || ActiveFeatures.IsModEnabled(ownerModId);

    public static Sprite GetAppBarIconFromTexture(Texture2D texture, int width = 0, int height = 0)
    {
        if (width == 0)
        {
            width = texture.width;
        }

        if (height == 0)
        {
            height = texture.height;
        }

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Removes every flight button, whose tray outlives the campaign, so the next flight builds the buttons the
    /// next campaign's features allow.
    /// </summary>
    internal static void RemoveFlightButtons()
    {
        foreach (var button in ButtonsToBeLoaded)
        {
            IAppbarBackend.Instance?.RemoveButton(button.ID);
        }
    }

    internal static void LoadAllButtons()
    {
        foreach (var button in ButtonsToBeLoaded.Where(button => ShowsButtonOf(button.owner)))
        {
            IAppbarBackend.Instance.AddButton(button.text, button.icon, button.ID, button.action);
        }
    }

    // ReSharper disable once InconsistentNaming
    internal static void LoadOABButtons()
    {
        foreach (var button in OabButtonsToBeLoaded.Where(button => ShowsButtonOf(button.owner)))
        {
            IAppbarBackend.Instance.AddOABButton(button.text, button.icon, button.ID, button.action);
        }
    }

    // ReSharper disable once InconsistentNaming
    internal static void LoadKSCButtons()
    {
        foreach (var button in KscButtonsToBeLoaded.Where(button => ShowsButtonOf(button.owner)))
        {
            IAppbarBackend.Instance.AddKSCButton(button.text, button.icon, button.ID, button.action);
        }
    }

    // A button registered through the old overloads belongs to the mod whose plugin lives in the calling assembly
    private static string OwnerOf(Assembly assembly) =>
        PluginList.AllEnabledAndActivePlugins
            .FirstOrDefault(descriptor => descriptor.Plugin != null && descriptor.Plugin.GetType().Assembly == assembly)
            ?.Guid;
}
