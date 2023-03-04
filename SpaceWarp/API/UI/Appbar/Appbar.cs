using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using UnityEngine;
using SpaceWarp.Backend.UI.Appbar;


namespace SpaceWarp.API.UI.Appbar;

public static class Appbar
{
    private static readonly List<(string text, Sprite icon, string ID, Action<bool> action)> ButtonsToBeLoaded = new();
    
    /// <summary>
    /// Register a appbar menu for the game
    /// </summary>
    /// <param name="text">The text in the appbar menu</param>
    /// <param name="title">The title of the menu</param>
    /// <param name="id">A unique id for the appbar menu eg: "BTN-Example"</param>
    /// <param name="icon">A Sprite for the icon in the appbar</param>
    /// <typeparam name="T">The type of the appbar menu, must extend AppbarMenu</typeparam>
    /// <returns>An instance of T which has been added to a GameObject</returns>
    public static T RegisterGameAppbarMenu<T>(string text, string title, string id, Sprite icon) where T : AppbarMenu
    {
        GameObject toolBarUIObject = new GameObject($"Toolbar: {id}");
        toolBarUIObject.Persist();
        AppbarMenu menu = toolBarUIObject.AddComponent<T>();
        menu.Title = title;
        menu.ID = id;
        toolBarUIObject.transform.SetParent(Chainloader.ManagerObject.transform);
        toolBarUIObject.SetActive(true);
        ButtonsToBeLoaded.Add((text, icon, id, menu.ToggleGUI));
        return menu as T;
    }

    /// <summary>
    /// Register a appbar menu for the game
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
    /// Register a button on the games AppBar
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
    /// Register a button on the games AppBar
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
    /// Convert a Texture2D to a Sprite
    /// </summary>
    /// <param name="texture">The Texture2D</param>
    /// <param name="width">The width of the sprite, 0 for inferring</param>
    /// <param name="height">The height of the sprite, 0 for inferring</param>
    /// <returns>The Texture2D converted to a Sprite</returns>
    public static Sprite GetAppBarIconFromTexture(Texture2D texture, int width=0, int height=0)
    {
        if (width == 0) width = texture.width;
        if (height == 0) height = texture.height;

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }
    
    
    internal static void LoadAllButtons()
    {
        foreach (var button in ButtonsToBeLoaded)
        {
            AppbarBackend.AddButton(button.text, button.icon, button.ID, button.action);
        }
    }
}
