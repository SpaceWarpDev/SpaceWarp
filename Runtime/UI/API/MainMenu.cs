using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SpaceWarp2.UI.API;

/// <summary>
/// Used to register buttons in the game's main menu.
/// </summary>
[PublicAPI]
public static class MainMenu
{
    public static readonly List<(string name, Action onClicked)> MenuButtonsToBeAdded = new();
    public static readonly List<(string term, Action onClicked)> LocalizedMenuButtonsToBeAdded = new();
    public static readonly List<(Func<string> termProvider, Action onClicked)> DynamicLocalizedMenuButtonsToBeAdded = new();

    public static event Action? DynamicLocalizedMenuButtonsChanged;

    /// <summary>
    /// Registers a button to be added to the main menu.
    /// </summary>
    /// <param name="name">The name of the button on the menu.</param>
    /// <param name="onClicked">The action that is invoked when the button is pressed</param>
    public static void RegisterMenuButton(string name, Action onClicked)
    {
        // Upsert by name. Registration re-runs on every Play Mode enter, and with Domain Reload disabled
        // these static lists persist across sessions, so a plain Add would append a duplicate button each
        // play (the main menu builder instantiates one button per entry). Replacing the same-name entry
        // also keeps the callback fresh (last registration wins) instead of pointing at destroyed objects.
        MenuButtonsToBeAdded.RemoveAll(b => b.name == name);
        MenuButtonsToBeAdded.Add((name, onClicked));
    }

    /// <summary>
    /// Registers a localized button to be added to the main menu.
    /// </summary>
    /// <param name="term">The term of the translation for button on the menu.</param>
    /// <param name="onClicked">The action that is invoked when the button is pressed</param>
    public static void RegisterLocalizedMenuButton(string term, Action onClicked)
    {
        // Upsert by term — see RegisterMenuButton for why (avoids duplicate buttons across Play Mode
        // sessions when Domain Reload is disabled).
        LocalizedMenuButtonsToBeAdded.RemoveAll(b => b.term == term);
        LocalizedMenuButtonsToBeAdded.Add((term, onClicked));
    }

    /// <summary>
    /// Registers a localized button whose term can change after the main menu has been built.
    /// </summary>
    /// <param name="termProvider">Function returning the current translation term for the button.</param>
    /// <param name="onClicked">The action that is invoked when the button is pressed</param>
    public static void RegisterDynamicLocalizedMenuButton(Func<string> termProvider, Action onClicked)
    {
        // Dynamic buttons have no stable string key, so de-duplicate by the provider's method (the same
        // method registered each Play Mode session — only the captured target instance differs). See
        // RegisterMenuButton for why upserting is needed when Domain Reload is disabled.
        DynamicLocalizedMenuButtonsToBeAdded.RemoveAll(b => Equals(b.termProvider.Method, termProvider.Method));
        DynamicLocalizedMenuButtonsToBeAdded.Add((termProvider, onClicked));
    }

    public static void RefreshDynamicLocalizedMenuButtons()
    {
        DynamicLocalizedMenuButtonsChanged?.Invoke();
    }

}