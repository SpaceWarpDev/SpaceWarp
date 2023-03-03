using KSP.Game;
using KSP.Sim.impl;
using SpaceWarp.API.Assets;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;

namespace SpaceWarp.UI;

public sealed class SpaceWarpConsole : KerbalMonoBehaviour
{
    private bool _drawUI;
    private Rect _windowRect;
    bool _autoScroll = true;

    private const ControlTypes ConsoleLocks = ControlTypes.All;
    private const string ConsoleLockID = "spacewarp.console";

    private int _windowWidth = 350;
    private int _windowHeight = 700;

    private static Vector2 _scrollPosition;
    private static Vector2 _scrollView;

    private string _search = "";

    private void Awake()
    {
        _windowWidth = (int)(Screen.width * 0.5f);
        _windowHeight = (int)(Screen.height * 0.5f);

        _windowRect = new Rect(Screen.width * 0.15f, Screen.height * 0.15f, 0, 0);
        _scrollPosition = Vector2.zero;
        Appbar.RegisterAppButton(
            "Console",
            "BTN-SWConsole",
            // Example of using the asset loader, were going to load the apps icon
            // Path format [mod_id]/images/filename
            // for bundles its [mod_id]/[bundle_name]/[path to file in bundle with out assets/bundle]/filename.extension
            // There is also a try get asset function, that returns a bool on whether or not it could grab the asset
            AssetManager.GetAsset<Texture2D>($"spacewarp/images/console.png"),
            ToggleVisible
            );
    }

    private void OnGUI()
    {
        GUI.skin = SpaceWarpManager.Skin;
        if (!_drawUI)
        {
            return;
        }

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        string header = "spacewarp.console";
        GUILayoutOption width = GUILayout.Width((float)(_windowWidth * 0.8));
        GUILayoutOption height = GUILayout.Height((float)(_windowHeight * 0.8));
        
        _windowRect = GUILayout.Window(controlID, _windowRect, DrawConsole, header, width, height);
    }
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C))
        {
            ToggleVisible(!_drawUI);
        }
    }

    private void DrawConsole(int windowID)
    {
        GUILayout.BeginVertical();
        _search = GUILayout.TextField(_search);
        _scrollView = GUILayout.BeginScrollView(_scrollPosition, false, true);
 
        foreach (string message in SpaceWarpConsoleLogListener.DebugMessages)
        {
            if (!message.ToLower().Contains(_search.ToLower())) continue;
            GUILayout.Label(message);
            if(_autoScroll)
            {
                _scrollView.Set(_scrollView.x, Mathf.Infinity);
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

        if (GUILayout.Button(_autoScroll ? "Auto Scroll: On" : "Auto Scroll: Off"))
        {
            //Todo: Add proper close button to top corner and add input lock button back. 
            // GameManager.Instance.Game.ViewController.inputLockManager.ClearControlLocks();
            _autoScroll = !_autoScroll;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 500));
    }
    public void ToggleVisible(bool shouldDraw)
    {
        _drawUI = shouldDraw;
        Game.ViewController.inputLockManager.SetControlLock(_drawUI ? ConsoleLocks : ControlTypes.None, ConsoleLockID);
    }
}