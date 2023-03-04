using KSP.Game;
using KSP.Sim.impl;
using KSP.UI.Binding;
using UnityEngine;

namespace SpaceWarp.API.UI.Appbar;

public abstract class AppbarMenu : KerbalBehavior
{
    private GUISkin _spaceWarpConsoleSkin;
    private bool _drawing;
    internal string ID;
    private GUIStyle _closeButtonStyle;

    public abstract float Width
    {
        get;
    }

    public abstract float Height
    {
        get;
    }

    public abstract float X
    {
        get;
    }

    public abstract float Y
    {
        get;
    }

    public virtual GUISkin Skin
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

    public string Title;
    private Rect _windowRect;

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
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        GUILayoutOption width = GUILayout.Width(Width);
        GUILayoutOption height = GUILayout.Height(Height);
            
        _windowRect = GUILayout.Window(controlID, _windowRect, DoDrawing, Title, width, height);
    }

    internal void ToggleGUI(bool drawing)
    {
        _drawing = drawing;
    }

    public void ToggleGUI()
    {
        _drawing = !_drawing;
    }

    private void DoDrawing(int windowID)
    {
        Rect closeButtonRect = new Rect(Width - 23, 6, 16, 16);
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
        GameObject.Find(ID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(false);
    }
}