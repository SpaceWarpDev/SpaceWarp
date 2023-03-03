using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using UnityEngine;
using SpaceWarp.Backend.UI.Appbar;


namespace SpaceWarp.API.UI.Appbar;

public static class Appbar
{
    private static readonly List<(string text, Sprite icon, string ID, Action<bool> action)> ButtonsToBeLoaded = new();

    public static T RegisterGameToolbarMenu<T>(string text, string title, string id, Sprite icon) where T : AppbarMenu
    {
        GameObject toolBarUIObject = new GameObject($"Toolbar: {id}");
        toolBarUIObject.Persist();
        AppbarMenu menu = toolBarUIObject.AddComponent<T>();
        menu.Title = title;
        toolBarUIObject.transform.SetParent(Chainloader.ManagerObject.transform);
        toolBarUIObject.SetActive(true);
        ButtonsToBeLoaded.Add((text, icon, id, menu.ToggleGUI));
        return menu as T;
    }

    public static T RegisterGameToolbarMenu<T>(string text, string title, string id, Texture2D icon) where T : AppbarMenu =>
        RegisterGameToolbarMenu<T>(text, title, id, GetToolBarIconFromTexture(icon));
    
    public static void RegisterAppButton(string text, string id, Sprite icon, Action<bool> func) => ButtonsToBeLoaded.Add((text ,icon, id, func));

    public static void RegisterAppButton(string text, string id, Texture2D icon, Action<bool> func) =>
        RegisterAppButton(text, id, GetToolBarIconFromTexture(icon), func);
    
    public static Sprite GetToolBarIconFromTexture(Texture2D texture, int width=0,int height=0)
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
