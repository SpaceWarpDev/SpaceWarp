using System;
using System.Collections.Generic;

namespace SpaceWarp.API.UI;

public static class MainMenu
{
    internal static List<(string name, Action onClicked)> MenuButtonsToBeAdded = new ();

    public static void RegisterMenuButton(string name, Action onClicked)
    {
        MenuButtonsToBeAdded.Add((name,onClicked));
    }
}