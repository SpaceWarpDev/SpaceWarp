using System.Collections.Generic;
using KSP.Game;
using SpaceWarp.API.Mods.JSON;
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
    private static Vector2 _scrollPositionInfo;

    private ModInfo _selectedMetaData = null;

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
        _boxStyle = GUI.skin.GetStyle("Box");
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();

        _scrollPositionMods = GUILayout.BeginScrollView(_scrollPositionMods, false, false,
            GUILayout.Height((float)(_windowHeight * 0.8)), GUILayout.Width(300));
        foreach (var mod in SpaceWarpManager.SpaceWarpPlugins)
        {
            if (GUILayout.Button(mod.SpaceWarpMetadata.Name))
            {
                _selectedMetaData = mod.SpaceWarpMetadata;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        if (_selectedMetaData != null)
        {
            GUILayout.BeginVertical();
            _scrollPositionInfo = GUILayout.BeginScrollView(_scrollPositionInfo, false, false);
            GUILayout.Label($"{_selectedMetaData.Name} (id: {_selectedMetaData.ModID})");
            GUILayout.Label($"Author: {_selectedMetaData.Author}");
            GUILayout.Label($"Version: {_selectedMetaData.Version}");
            GUILayout.Label($"Source: {_selectedMetaData.Source}");
            GUILayout.Label($"Description: {_selectedMetaData.Description}");
            GUILayout.Label($"KSP2 Version: {_selectedMetaData.SupportedKsp2Versions.Min} - {_selectedMetaData.SupportedKsp2Versions.Max}");
            GUILayout.Label($"Dependencies");

            foreach (DependencyInfo dependency in _selectedMetaData.Dependencies)
            {
                GUILayout.Label($"{dependency.ID}: {dependency.Version.Min} - {dependency.Version.Max}");
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Close"))
        {
            ToggleVisible();
        }
        GUI.DragWindow();
    }

    public void ToggleVisible()
    {
        _drawUI = !_drawUI;
    }
}