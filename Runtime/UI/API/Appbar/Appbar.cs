using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SpaceWarp.UI.Backend.UI.Appbar;
using UnityEngine;

namespace SpaceWarp.UI.API.Appbar;

/// <summary>
/// Used to register buttons on the game's AppBar.
/// </summary>
[PublicAPI]
public static class Appbar
{
    private static readonly List<(string text, Sprite icon, string ID, Action<bool> action)> ButtonsToBeLoaded = new();

    private static readonly List<(string text, Sprite icon, string ID, Action<bool> action)> OabButtonsToBeLoaded = new();

    private static readonly List<(string text, Sprite icon, string ID, Action action)> KscButtonsToBeLoaded = new();

    /// <summary>
    /// Register a button on the game's AppBar
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-Example"</param>
    /// <param name="icon">A Sprite for the icon in the appbar</param>
    /// <param name="func">The function to be called when this button is clicked</param>
    public static void RegisterAppButton(string text, string id, Sprite icon, Action<bool> func)
    {
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"RegisterAppButton: {text}, {id}");
        ButtonsToBeLoaded.Add((text, icon, id, func));
    }

    /// <summary>
    /// Register a button on the game's AppBar
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-Example"</param>
    /// <param name="icon">A Texture2D for the icon in the appbar</param>
    /// <param name="func">The function to be called when this button is clicked</param>
    public static void RegisterAppButton(string text, string id, Texture2D icon, Action<bool> func)
    {
        RegisterAppButton(text, id, GetAppBarIconFromTexture(icon), func);
    }

    /// <summary>
    /// Register a button on the OAB AppBar
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-ExampleOAB"</param>
    /// <param name="icon">A Sprite for the icon in the appbar</param>
    /// <param name="func">The function to be called when this button is clicked</param>
    // ReSharper disable once InconsistentNaming
    public static void RegisterOABAppButton(string text, string id, Sprite icon, Action<bool> func)
    {
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"RegisterOABAppButton: {text}, {id}");
        OabButtonsToBeLoaded.Add((text, icon, id, func));
    }

    /// <summary>
    /// Register a button on the OAB AppBar
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-ExampleOAB"</param>
    /// <param name="icon">A Texture2D for the icon in the appbar</param>
    /// <param name="func">The function to be called when this button is clicked</param>
    // ReSharper disable once InconsistentNaming
    public static void RegisterOABAppButton(string text, string id, Texture2D icon, Action<bool> func)
    {
        RegisterOABAppButton(text, id, GetAppBarIconFromTexture(icon), func);
    }

    /// <summary>
    /// Register a button on the KSC AppBar
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-ExampleKSC"</param>
    /// <param name="icon">A Sprite for the icon in the appbar</param>
    /// <param name="func">The function to be called when this button is clicked</param>
    // ReSharper disable once InconsistentNaming
    public static void RegisterKSCAppButton(string text, string id, Sprite icon, Action func)
    {
        KscButtonsToBeLoaded.Add((text, icon, id, func));
    }

    /// <summary>
    /// Register a button on the KSC AppBar
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-ExampleKSC"</param>
    /// <param name="icon">A Texture2D for the icon in the appbar</param>
    /// <param name="func">The function to be called when this button is clicked</param>
    // ReSharper disable once InconsistentNaming
    public static void RegisterKSCAppButton(string text, string id, Texture2D icon, Action func)
    {
        RegisterKSCAppButton(text, id, GetAppBarIconFromTexture(icon), func);
    }

    /// <summary>
    /// Convert a Texture2D to a Sprite
    /// </summary>
    /// <param name="texture">The Texture2D</param>
    /// <param name="width">The width of the sprite, 0 for inferring</param>
    /// <param name="height">The height of the sprite, 0 for inferring</param>
    /// <returns>The Texture2D converted to a Sprite</returns>
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

    internal static void LoadAllButtons()
    {
        foreach (var button in ButtonsToBeLoaded)
        {
            IAppbarBackend.Instance.AddButton(button.text, button.icon, button.ID, button.action);
        }
    }

    // ReSharper disable once InconsistentNaming
    internal static void LoadOABButtons()
    {
        foreach (var button in OabButtonsToBeLoaded)
        {
            IAppbarBackend.Instance.AddOABButton(button.text, button.icon, button.ID, button.action);
        }
    }

    // ReSharper disable once InconsistentNaming
    internal static void LoadKSCButtons()
    {
        foreach (var button in KscButtonsToBeLoaded)
        {
            IAppbarBackend.Instance.AddKSCButton(button.text, button.icon, button.ID, button.action);
        }
    }
}