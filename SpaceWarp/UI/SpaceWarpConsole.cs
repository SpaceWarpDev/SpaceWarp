using System.Collections.Generic;
using KSP.Sim.impl;
using UnityEngine;
using SpaceWarp.API;

namespace SpaceWarp.UI;

public class SpaceWarpConsole : KerbalBehavior
{
    private static bool _loaded;

    private bool _drawUI;
    private Rect _windowRect;
    bool _autoScroll = true;

    private int _windowWidth = 350;
    private int _windowHeight = 700;

    private static GUIStyle _boxStyle;
    private static Vector2 _scrollPosition;
    private static Vector2 _scrollView;

    private readonly Queue<string> _debugMessages = new();

    public new void Start()
    {
        if (_loaded)
        {
            Destroy(this);
        }

        _loaded = true;
    }

    private new void Awake()
    {

        _windowWidth = (int)(Screen.width * 0.5f);
        _windowHeight = (int)(Screen.height * 0.5f);

        _windowRect = new Rect((Screen.width * 0.15f), (Screen.height * 0.15f), 0, 0);
        _scrollPosition = Vector2.zero;
        
    }

    private void OnGUI()
    {
        GUI.skin = SpaceWarpManager.Skin;
        if (!_drawUI)
        {
            return;
        }

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        string header = $"spacewarp.console";
        GUILayoutOption width = GUILayout.Width((float)(_windowWidth * 0.8));
        GUILayoutOption height = GUILayout.Height((float)(_windowHeight * 0.8));
        
        _windowRect = GUILayout.Window(controlID, _windowRect, DrawConsole, header, width, height);
    }
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C))
        {
            _drawUI = !_drawUI;
        }
    }

    private void DrawConsole(int windowID)
    {
        _boxStyle = GUI.skin.GetStyle("Box");
        GUILayout.BeginVertical();
        _scrollView = GUILayout.BeginScrollView(_scrollPosition, false, true);
 
        foreach (string message in SpaceWarpConsoleLogListener.DebugMessages)
        {
            string new_message = "" + message + "\n";
            GUILayout.Label( new_message);
            if(_autoScroll)
            {
                _scrollView.Set(_scrollView.x, Mathf.Infinity );
                _scrollPosition = _scrollView;
            }
            else
            {
                _scrollPosition = _scrollView;
            }
        }
        
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Close"))
        {
            _drawUI = false;
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("Clear"))
        {
            SpaceWarpConsoleLogListener.DebugMessages.Clear();
        }

        if (GUILayout.Button( _autoScroll ? "Auto Scroll: On" : "Auto Scroll: Off" ))
        {
            //Todo: Add proper close button to top corner and add input lock button back. 
            // GameManager.Instance.Game.ViewController.inputLockManager.ClearControlLocks();
            _autoScroll = !_autoScroll;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 500));
    }
}