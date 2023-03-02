using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using UnityEngine;

namespace SpaceWarp.API.Toolbar;

public static class Toolbar
{
    private static readonly List<(string text, Sprite icon, string ID, Action<bool> action)> _buttonsToBeLoaded = new();

    public static T RegisterGameToolbarMenu<T>(string text, string title, string id, Sprite icon) where T : ToolbarMenu
    {
        GameObject toolBarUIObject = new GameObject($"Toolbar: {id}");
        toolBarUIObject.Persist();
        ToolbarMenu menu = toolBarUIObject.AddComponent<T>();
        menu.Title = title;
        toolBarUIObject.transform.SetParent(Chainloader.ManagerObject.transform);
        toolBarUIObject.SetActive(true);
        _buttonsToBeLoaded.Add((text, icon, id, menu.ToggleGUI));
        return menu as T;
    }
    
    public static void RegisterAppButton(string text, string id, Sprite icon, Action<bool> func) => _buttonsToBeLoaded.Add((text ,icon, id, func));
    
    internal static void LoadAllButtons()
    {
        foreach (var button in _buttonsToBeLoaded)
        {
            ToolbarBackend.AddButton(button.text, button.icon, button.ID, button.action);
        }
    }
}
