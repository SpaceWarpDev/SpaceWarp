using System.Collections.Generic;
using KSP.Game;
using UnityEngine;

namespace SpaceWarp.UI;

public class ModListUI : KerbalMonoBehaviour
{
    private static bool _loaded;
    private bool _drawUI;
    private Rect _windowRect;

    private int _windowWidth = 350;
    private int _windowHeight = 700;

    private static GUIStyle _boxStyle;
    private static Vector2 _scrollPositionMods;
        
    private List<(string, bool)> _toggles = new List<(string, bool)>();
    private List<(string, bool)> _initialToggles = new List<(string, bool)>();
    private readonly Dictionary<string, bool> _wasToggledDict = new Dictionary<string, bool>();

    public void Start()
    {
        if (_loaded)
        {
            Destroy(this);
        }

        _loaded = true;
    }

    private void Awake()
    {
        _windowWidth = (int)(Screen.width * 0.85f);
        _windowHeight = (int)(Screen.height * 0.85f);

        _windowRect = new Rect(Screen.width * 0.15f, Screen.height * 0.15f, 0, 0);
    }

    private void OnGUI()
    {
        GUI.skin = SpaceWarpManager.Skin;
        if (!_drawUI)
        {
            return;
        }

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        const string header = "spacewarp.modlist";
        GUILayoutOption width = GUILayout.Width((float)(_windowWidth * 0.8));
        GUILayoutOption height = GUILayout.Height((float)(_windowHeight * 0.8));
        GUI.skin = SpaceWarpManager.Skin;

        _windowRect = GUILayout.Window(controlID, _windowRect, FillWindow, header, width, height);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.M))
        {
            ToggleVisible();
        }
    }

    private void FillWindow(int windowID)
    {
        if (_initialToggles.Count == 0)
        {
            _initialToggles = new List<(string, bool)>(_toggles);
        }
            
        _boxStyle = GUI.skin.GetStyle("Box");
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();

        _scrollPositionMods = GUILayout.BeginScrollView(_scrollPositionMods, false, true,
            GUILayout.Height((float)(_windowHeight * 0.8)), GUILayout.Width(300));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Disable All"))
        {
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i] = (_toggles[i].Item1, false);
            }
        }

        if (GUILayout.Button("Enable All"))
        {
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i] = (_toggles[i].Item1, true);
            }
        }
        GUILayout.EndHorizontal();

        int numChanges = 0;
        for (int i = 0; i < _toggles.Count; i++)
        {
            if (_toggles[i].Item2 != _initialToggles[i].Item2)
            {
                numChanges++;
            }
        }
    
        if (numChanges > 0)
        {
            GUILayout.Label($"{numChanges} changes detected, please restart");
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    public void ToggleVisible()
    {
        _drawUI = !_drawUI;
    }
}