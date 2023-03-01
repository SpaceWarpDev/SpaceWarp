using System;
using System.Collections.Generic;
using System.IO;
using KSP.Game;
using SpaceWarp.API;
using SpaceWarp.API.AssetBundles;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Managers;
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
    private string _selectedMod;
    private ModInfo _selectedModInfo;
    private GUISkin _spaceWarpUISkin;
        
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
        ResourceManager.TryGetAsset($"space_warp/swconsoleui/swconsoleUI/spacewarpConsole.guiskin", out _spaceWarpUISkin);
    }

    private void OnGUI()
    {
        GUI.skin = _spaceWarpUISkin;
        if (!_drawUI)
        {
            return;
        }

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        const string header = "spacewarp.modlist";
        GUILayoutOption width = GUILayout.Width((float)(_windowWidth * 0.8));
        GUILayoutOption height = GUILayout.Height((float)(_windowHeight * 0.8));
        GUI.skin = _spaceWarpUISkin;

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
            

        GUILayout.BeginHorizontal();
        if (ManagerLocator.TryGet(out SpaceWarpManager managerRemove))
        {
            if (GUILayout.Button("Revert Changes"))
            {
                // Replace _toggles list with backup copy
                _toggles = new List<(string, bool)>(_initialToggles);

                // Delete all ignore files
                foreach ((string modID, ModInfo modInfo) in managerRemove.LoadedMods)
                {
                    if (File.Exists($"SpaceWarp/Mods/{modID}/.ignore"))
                    {
                        File.Delete($"SpaceWarp/Mods/{modID}/.ignore");
                    }
                }
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

        if (ManagerLocator.TryGet(out SpaceWarpManager manager))
        {
            foreach ((string modID, ModInfo modInfo) in manager.LoadedMods)
            {
                int toggleIndex = _toggles.FindIndex(t => t.Item1 == modID);
                if (toggleIndex == -1) // Toggle not found, add a new one
                {
                    _toggles.Add((modID, true));
                    toggleIndex = _toggles.Count - 1;
                }

                bool isToggled = _toggles[toggleIndex].Item2; // current state of the toggle
                bool wasToggled = _wasToggledDict.ContainsKey(modID) && _wasToggledDict[modID]; // previous state of the toggle (defaults to false if not found)

                GUILayout.BeginHorizontal();
                _toggles[toggleIndex] = (modID, GUILayout.Toggle(isToggled, ""));
                if (GUILayout.Button(modID))
                {
                    _selectedMod = modID;
                    _selectedModInfo = modInfo;
                }
                GUILayout.EndHorizontal();

                // Edge detection
                if (!isToggled && wasToggled) // falling edge
                {
                    File.Create($"SpaceWarp/Mods/{modID}/.ignore").Close();
                }
                else if (isToggled && !wasToggled) // rising edge
                {
                    File.Delete($"SpaceWarp/Mods/{modID}/.ignore");
                }

                _wasToggledDict[modID] = isToggled; // update the previous state of the toggle
            }

                
            foreach ((string modID, ModInfo modInfo) in manager.IgnoredMods)
            {
                int toggleIndex = _toggles.FindIndex((t) => t.Item1 == modID);
                if (toggleIndex == -1) // Toggle not found, add a new one
                {
                    _toggles.Add((modID, false));
                    toggleIndex = _toggles.Count - 1;
                }

                bool isToggled = _toggles[toggleIndex].Item2; // current state of the toggle
                bool wasToggled = !_wasToggledDict.ContainsKey(modID) || _wasToggledDict[modID];
                    
                GUILayout.BeginHorizontal();

                // Add a space to vertically center the toggle button
                GUILayoutOption[] alignMiddleOption = { GUILayout.Height(30)};

                _toggles[toggleIndex] = (modID, GUILayout.Toggle(isToggled, "", alignMiddleOption));
                if (GUILayout.Button(modID))
                {
                    _selectedMod = modID;
                    _selectedModInfo = modInfo;
                }

                GUILayout.EndHorizontal();
                // Edge detection
                if (isToggled && !wasToggled) // falling edge
                {
                    File.Delete($"SpaceWarp/Mods/{modID}/.ignore");
                }
                else if (!isToggled && wasToggled) // rising edge
                {
                    File.Create($"SpaceWarp/Mods/{modID}/.ignore").Close();
                }

                _wasToggledDict[modID] = isToggled; // update the previous state of the toggle
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.BeginVertical();

        if (_selectedModInfo != null)
        {
            CreateModConfigurationUI();
        }
        else
        {
            GUILayout.Label("No mod selected");
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    private void CreateModConfigurationUI()
    {
        GUILayout.Label(_selectedModInfo.name);
        GUILayout.Label($"Author: {_selectedModInfo.author}");
        GUILayout.Label($"Version: {_selectedModInfo.version}");
        GUILayout.Label($"Source: {_selectedModInfo.source}");
        GUILayout.Label($"Description: {_selectedModInfo.description}");
        GUILayout.Label($"KSP2 Version: {_selectedModInfo.supported_ksp2_versions.min} - {_selectedModInfo.supported_ksp2_versions.max}");
        GUILayout.Label($"Dependencies");

        foreach (DependencyInfo dependency in _selectedModInfo.dependencies)
        {
            GUILayout.Label($"{dependency.id}: {dependency.version.min} - {dependency.version.max}");
        }

        if (!ManagerLocator.TryGet(out ConfigurationManager configManager))
        {
            return;
        }

        if (!configManager.TryGet(_selectedModInfo.mod_id, out (Type configType, object configObject, string path) config))
        {
            return;
        }

        if (!GUILayout.Button("Configure"))
        {
            return;
        }

        GameObject go = new GameObject(_selectedModInfo.mod_id);
        go.transform.SetParent(transform);

        ModConfigurationUI configUI = go.AddComponent<ModConfigurationUI>();

        configUI.ConfigurationType = config.configType;
        configUI.ConfigurationObject = config.configObject;
        configUI.modID = _selectedMod;

            
        go.SetActive(true);
    }

    public void ToggleVisible()
    {
        _drawUI = !_drawUI;
    }
}