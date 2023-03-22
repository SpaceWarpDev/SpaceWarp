using KSP.Game;
using KSP.Sim.impl;
using UnityEngine;

namespace SpaceWarp.API.UI.Appbar;

public abstract class AppbarMenu : KerbalBehavior
{
    public string Title;
    private GUIStyle _closeButtonStyle;
    private bool _drawing;
    private GUISkin _spaceWarpConsoleSkin;
    private Rect _windowRect;
    internal string ID;

    public abstract float Width { get; }

    public abstract float Height { get; }

    public abstract float X { get; }

    public abstract float Y { get; }

    public virtual GUISkin Skin
    {
        get
        {
            if (_spaceWarpConsoleSkin == null) _spaceWarpConsoleSkin = Skins.ConsoleSkin;

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
            || GameManager.Instance.Game.GlobalGameState.GetState() != GameState.FlightView) return;

        GUI.skin = Skin;
        _closeButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            fontSize = 8
        };
        var controlID = GUIUtility.GetControlID(FocusType.Passive);

        var width = GUILayout.Width(Width);
        var height = GUILayout.Height(Height);

        _windowRect = GUILayout.Window(controlID, _windowRect, DoDrawing, Title, width, height);
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