using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using KSP.UI.Binding;
using SpaceWarp.Backend.UI.Appbar;
using SpaceWarp.InternalUtilities;
using UnityEngine;

namespace SpaceWarp.API.UI.Appbar;

public static class Appbar
{
    private static readonly List<(string text, Sprite icon, string ID, Action<bool> action)> ButtonsToBeLoaded = new();

    private static readonly List<(string text, Sprite icon, string ID, Action<bool> action)> OABButtonsToBeLoaded =
        new();

    /// <summary>
    ///     Register a appbar menu for the game
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="title">The title of the menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-Example"</param>
    /// <param name="icon">A Sprite for the icon in the appbar</param>
    /// <typeparam name="T">The type of the appbar menu, must extend AppbarMenu</typeparam>
    /// <returns>An instance of T which has been added to a GameObject</returns>
    public static T RegisterGameAppbarMenu<T>(string text, string title, string id, Sprite icon) where T : AppbarMenu
    {
        var toolBarUIObject = new GameObject($"Toolbar: {id}");
        toolBarUIObject.Persist();
        AppbarMenu menu = toolBarUIObject.AddComponent<T>();
        menu.title = title;
        menu.ID = id;
        toolBarUIObject.transform.SetParent(Chainloader.ManagerObject.transform);
        toolBarUIObject.SetActive(true);
        ButtonsToBeLoaded.Add((text, icon, id, menu.ToggleGUI));
        return menu as T;
    }

    /// <summary>
    ///     Register a appbar menu for the game
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="title">The title of the menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-Example"</param>
    /// <param name="icon">A Texture2D for the icon in the appbar</param>
    /// <typeparam name="T">The type of the appbar menu, must extend AppbarMenu</typeparam>
    /// <returns>An instance of T which has been added to a GameObject</returns>
    public static T RegisterGameAppbarMenu<T>(string text, string title, string id, Texture2D icon) where T : AppbarMenu
    {
        return RegisterGameAppbarMenu<T>(text, title, id, GetAppBarIconFromTexture(icon));
    }

    /// <summary>
    ///     Register a button on the games AppBar
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-Example"</param>
    /// <param name="icon">A Sprite for the icon in the appbar</param>
    /// <param name="func">The function to be called when this button is clicked</param>
    public static void RegisterAppButton(string text, string id, Sprite icon, Action<bool> func)
    {
        ButtonsToBeLoaded.Add((text, icon, id, func));
    }

    /// <summary>
    ///     Register a button on the games AppBar
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
    ///     Register a button on the OAB AppBar
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-ExampleOAB"</param>
    /// <param name="icon">A Sprite for the icon in the appbar</param>
    /// <param name="func">The function to be called when this button is clicked</param>
    public static void RegisterOABAppButton(string text, string id, Sprite icon, Action<bool> func)
    {
        OABButtonsToBeLoaded.Add((text, icon, id, func));
    }

    /// <summary>
    ///     Register a button on the OAB AppBar
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-ExampleOAB"</param>
    /// <param name="icon">A Texture2D for the icon in the appbar</param>
    /// <param name="func">The function to be called when this button is clicked</param>
    public static void RegisterOABAppButton(string text, string id, Texture2D icon, Action<bool> func)
    {
        RegisterOABAppButton(text, id, GetAppBarIconFromTexture(icon), func);
    }

    /// <summary>
    ///     Convert a Texture2D to a Sprite
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


    /// <summary>
    ///     Sets an app bar buttons indicator (the green sprite to the side of it)
    /// </summary>
    /// <param name="id">The id of the button, what you set when registering the app bar button</param>
    /// <param name="indicator">The state of the indicator, true for on, false for off</param>
    public static void SetAppBarButtonIndicator(string id, bool indicator)
    {
        GameObject.Find(id)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(indicator);
    }

    internal static void LoadAllButtons()
    {
        foreach (var button in ButtonsToBeLoaded)
        {
            AppbarBackend.AddButton(button.text, button.icon, button.ID, button.action);
        }
    }

    internal static void LoadOABButtons()
    {
        foreach (var button in OABButtonsToBeLoaded)
        {
            AppbarBackend.AddOABButton(button.text, button.icon, button.ID, button.action);
        }
    }
}