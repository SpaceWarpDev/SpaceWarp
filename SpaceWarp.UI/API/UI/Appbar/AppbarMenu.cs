using System;
using KSP.Game;
using KSP.Sim.impl;
using UnityEngine;
using UnityEngine.Serialization;

namespace SpaceWarp.API.UI.Appbar;

[Obsolete("Spacewarps support for IMGUI will not be getting updates, please use UITK instead")]
public abstract class AppbarMenu : KerbalBehavior
{
    [FormerlySerializedAs("Title")] public string title;
    private GUIStyle _closeButtonStyle;
    private bool _drawing;
    private GUISkin _spaceWarpConsoleSkin;
    private Rect _windowRect;
    internal string ID;

    protected abstract float Width { get; }

    protected abstract float Height { get; }

    protected abstract float X { get; }

    protected abstract float Y { get; }

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

    public new void Awake()
    {
        _windowRect = new Rect(X, Y, 0, 0);
    }

    public void OnGUI()
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

    public void ToggleGUI()
    {
        _drawing = !_drawing;
    }

    private void DoDrawing(int windowID)
    {
        var closeButtonRect = new Rect(Width - 23, 6, 16, 16);
        if (GUI.Button(new Rect(_windowRect.width - 18, 2, 16, 16), "x", _closeButtonStyle))
        {
            CloseWindow();
            GUIUtility.ExitGUI();
        }

        DrawWindow(windowID);
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }

    public abstract void DrawWindow(int windowID);

    public void CloseWindow()
    {
        ToggleGUI(false);
    }
}