using System;
using System.Collections.Generic;

namespace SpaceWarp.API.UI;

public static class MainMenu
{
    internal static readonly List<(string name, Action onClicked)> MenuButtonsToBeAdded = new();
    internal static readonly List<(string term, Action onClicked)> LocalizedMenuButtonsToBeAdded = new();

    /// <summary>
    ///     Registers a button to be added to the main menu.
    /// </summary>
    /// <param name="name">The name of the button on the menu.</param>
    /// <param name="onClicked">The action that is invoked when the button is pressed</param>
    public static void RegisterMenuButton(string name, Action onClicked)
    {
        MenuButtonsToBeAdded.Add((name, onClicked));
    }
    
    /// <summary>
    ///     Registers a localized button to be added to the main menu.
    /// </summary>
    /// <param name="term">The term of the translation for button on the menu.</param>
    /// <param name="onClicked">The action that is invoked when the button is pressed</param>
    public static void RegisterLocalizedMenuButton(string term, Action onClicked)
    {
        LocalizedMenuButtonsToBeAdded.Add((term, onClicked));
    }
    
}