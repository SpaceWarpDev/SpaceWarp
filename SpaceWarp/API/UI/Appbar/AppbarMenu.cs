using KSP.Game;
using KSP.Sim.impl;
using SpaceWarp.API.Assets;
using UnityEngine;

namespace SpaceWarp.API.UI.Appbar;

public abstract class AppbarMenu : KerbalBehavior
{
    private GUISkin _spaceWarpConsoleSkin = null;
    private bool _drawing = false;
        
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
                AssetManager.TryGetAsset($"space_warp/swconsoleui/swconsoleUI/spacewarpConsole.guiskin", out _spaceWarpConsoleSkin);
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
        DrawWindow(windowID);
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }
        
    public abstract void DrawWindow(int windowID);
}