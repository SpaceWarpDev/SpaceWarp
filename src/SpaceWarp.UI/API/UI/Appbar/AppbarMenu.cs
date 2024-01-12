using JetBrains.Annotations;
using KSP.Game;
using KSP.Sim.impl;
using UnityEngine;
using UnityEngine.Serialization;

namespace SpaceWarp.API.UI.Appbar;

/// <summary>
/// Used to create a menu on the game's AppBar.
/// </summary>
[Obsolete("Spacewarps support for IMGUI will not be getting updates, please use UITK instead")]
[PublicAPI]
public abstract class AppbarMenu : KerbalBehavior
{
    /// <summary>
    /// The title of the menu
    /// </summary>
    // ReSharper disable once InconsistentNaming
    [FormerlySerializedAs("Title")] public string title;

    private GUIStyle _closeButtonStyle;
    private bool _drawing;
    private GUISkin _spaceWarpConsoleSkin;
    private Rect _windowRect;
    internal string ID;

    /// <summary>
    /// The width of the menu
    /// </summary>
    protected abstract float Width { get; }

    /// <summary>
    /// The height of the menu
    /// </summary>
    protected abstract float Height { get; }

    /// <summary>
    /// The X position of the menu
    /// </summary>
    protected abstract float X { get; }

    /// <summary>
    /// The Y position of the menu
    /// </summary>
    protected abstract float Y { get; }

    /// <summary>
    /// The skin to use for the menu
    /// </summary>
    protected virtual GUISkin Skin
    {
        get
        {
            if (_spaceWarpConsoleSkin == null)
            {
                _spaceWarpConsoleSkin = Skins.ConsoleSkin;
            }

            return _spaceWarpConsoleSkin;
        }
    }

    /// <summary>
    /// Called when the menu is created
    /// </summary>
    protected new void Awake()
    {
        _windowRect = new Rect(X, Y, 0, 0);
    }

    /// <summary>
    /// Draws the GUI
    /// </summary>
    protected void OnGUI()
    {
        if (!_drawing
            || GameManager.Instance.Game.GlobalGameState.GetState() != GameState.FlightView)
        {
            return;
        }

        GUI.skin = Skin;
        _closeButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            fontSize = 8
        };
        var controlID = GUIUtility.GetControlID(FocusType.Passive);

        var width = GUILayout.Width(Width);
        var height = GUILayout.Height(Height);

        _windowRect = GUILayout.Window(controlID, _windowRect, DoDrawing, title, width, height);
    }

    internal void ToggleGUI(bool drawing)
    {
        _drawing = drawing;
        Appbar.SetAppBarButtonIndicator(ID, drawing);
    }

    /// <summary>
    /// Toggles the menu
    /// </summary>
    public void ToggleGUI()
    {
        _drawing = !_drawing;
    }

    private void DoDrawing(int windowID)
    {
        if (GUI.Button(new Rect(_windowRect.width - 18, 2, 16, 16), "x", _closeButtonStyle))
        {
            CloseWindow();
            GUIUtility.ExitGUI();
        }

        DrawWindow(windowID);
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }

    /// <summary>
    /// Contains the code to draw the menu
    /// </summary>
    /// <param name="windowID">The ID of the window</param>
    public abstract void DrawWindow(int windowID);

    /// <summary>
    /// Closes the menu
    /// </summary>
    public void CloseWindow()
    {
        ToggleGUI(false);
    }
}