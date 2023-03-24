using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using ChainloaderPatcher;
using I2.Loc;
using KSP.Game;
using SpaceWarp.API.Mods.JSON;
using UnityEngine;

namespace SpaceWarp.UI;

public class ModListUI : KerbalMonoBehaviour
{
    // private const string ModListHeader = "spacewarp.modlist";
    private static LocalizedString _enableAll = "SpaceWarp/ModList/EnableAll";
    private static LocalizedString _disableAll = "SpaceWarp/ModList/DisableAll";
    private static LocalizedString _revertChanges = "SpaceWarp/ModList/RevertChanges";
    private static LocalizedString _modListHeader = "SpaceWarp/ModList/Header";
    private static LocalizedString _spaceWarpMods = "SpaceWarp/ModList/SpaceWarpMods";
    private static LocalizedString _unmanagedMods = "SpaceWarp/ModList/UnmanagedMods";
    private static LocalizedString _disabledMods = "SpaceWarp/ModList/DisabledMods";
    private static LocalizedString _version = "SpaceWarp/ModList/Version";
    private static LocalizedString _author = "SpaceWarp/ModList/Author";
    private static LocalizedString _outdated = "SpaceWarp/ModList/outdated";
    private static LocalizedString _source = "SpaceWarp/ModList/Source";
    private static LocalizedString _description = "SpaceWarp/ModList/Description";
    private static LocalizedString _ksp2Version = "SpaceWarp/ModList/KSP2Version";
    private static LocalizedString _dependencies = "SpaceWarp/ModList/Dependencies";
    private static LocalizedString _openConfigManager = "SpaceWarp/ModList/OpenConfigManager";
    private static LocalizedString _unsupported = "SpaceWarp/ModList/unsupported";
    
    private static bool _loaded;
    
    private bool _drawUI;
    private int _windowHeight = 700;
    private int _windowWidth = 350;
    private Rect _windowRect;

    private static GUIStyle _boxStyle;
    private static Vector2 _scrollPositionMods;
    private static Vector2 _scrollPositionInfo;
    private static GUIStyle _closeButtonStyle;
    private static GUIStyle _outdatedModStyle;
    private static GUIStyle _unsupportedModStyle;
    private static GUIStyle _headerStyle;

    private bool _showSupportedMods = true;
    private bool _showUnmanagedMods = true;
    private bool _showDisabledMods = true;

    private bool _selectedBepIn;
    private BepInPlugin _selectedBepInMetadata;
    private ModInfo _selectedMetaData;
    
    private List<(string, bool)> _toggles = new();
    private List<(string, bool)> _initialToggles = new();
    private readonly Dictionary<string, bool> _wasToggledDict = new();

    private void Awake()
    {
        const float minResolution = 1280f / 720f;
        const float maxResolution = 2048f / 1080f;
        var screenRatio = Screen.width / (float)Screen.height;
        var scaleFactor = Mathf.Clamp(screenRatio, minResolution, maxResolution);

        _windowWidth = (int)(Screen.width * 0.5f * scaleFactor);
        _windowHeight = (int)(Screen.height * 0.5f * scaleFactor);
        _windowRect = new Rect(
            Screen.width * 0.15f,
            Screen.height * 0.15f,
            Screen.width * 0.5f * scaleFactor,
            Screen.height * 0.5f * scaleFactor
        );
    }

    public void Start()
    {
        if (_loaded) Destroy(this);

        _loaded = true;
        _initialToggles = SpaceWarpManager.PluginGuidEnabledStatus.ToList();
        _toggles = _initialToggles;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.M)) ToggleVisible();

        if (!Input.GetKey(KeyCode.Escape) || !_drawUI) return;
        CloseWindow();
        GUIUtility.ExitGUI();
    }

    private void OnGUI()
    {
        GUI.skin = SpaceWarpManager.Skin;
        if (!_drawUI) return;

        _closeButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            fontSize = 8
        };

        _outdatedModStyle ??= new GUIStyle(GUI.skin.button)
        {
            normal =
            {
                textColor = Color.yellow
            },
            active =
            {
                textColor = Color.yellow
            },
            hover =
            {
                textColor = Color.yellow
            },
            focused =
            {
                textColor = Color.yellow
            },
            onActive =
            {
                textColor = Color.yellow
            },
            onFocused =
            {
                textColor = Color.yellow
            },
            onHover =
            {
                textColor = Color.yellow
            },
            onNormal =
            {
                textColor = Color.yellow
            }
        };
        _unsupportedModStyle ??= new GUIStyle(GUI.skin.button)
        {
            normal =
            {
                textColor = Color.red
            },
            active =
            {
                textColor = Color.red
            },
            hover =
            {
                textColor = Color.red
            },
            focused =
            {
                textColor = Color.red
            },
            onActive =
            {
                textColor = Color.red
            },
            onFocused =
            {
                textColor = Color.red
            },
            onHover =
            {
                textColor = Color.red
            },
            onNormal =
            {
                textColor = Color.red
            }
        };
        _headerStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        var controlID = GUIUtility.GetControlID(FocusType.Passive);
        var width = GUILayout.Width((float)(_windowWidth * 0.8));
        var height = GUILayout.Height((float)(_windowHeight * 0.8));

        _windowRect = GUILayout.Window(controlID, _windowRect, FillWindow, _modListHeader, width, height);
    }

    private static string Trim(string name)
    {
        if (name.Length > 25)
        {
            name = name[..22] + "...";
        }

        return name;
    }
    
    private void FillWindow(int windowID)
    {
        _boxStyle = GUI.skin.GetStyle("Box");
        if (GUI.Button(new Rect(_windowRect.width - 18, 2, 16, 16), "x", _closeButtonStyle))
        {
            _drawUI = false;
            GUIUtility.ExitGUI();
        }

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();

        _scrollPositionMods = GUILayout.BeginScrollView(
            _scrollPositionMods,
            false,
            false,
            GUILayout.Height((float)(_windowHeight * 0.8)),
            GUILayout.Width(300)
        );
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(_disableAll))
        {
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i] = (_toggles[i].Item1, false);
            }
        }
        
        if (GUILayout.Button(_enableAll))
        {
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i] = (_toggles[i].Item1, true);
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(_revertChanges))
        {
            // Replace _toggles list with backup copy
            _toggles = new List<(string, bool)>(_initialToggles);
            UpdateDisabledFile();
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
            GUILayout.Label($"{numChanges} changes detected, please restart to apply them");
        }

        
        if (_showSupportedMods)
        {
            if (GUILayout.Button($"{_spaceWarpMods} ▼", _headerStyle))
            {
                _showSupportedMods = !_showSupportedMods;
            }
        }
        else
        {
            if (GUILayout.Button($"{_spaceWarpMods} ▲", _headerStyle))
            {
                _showSupportedMods = !_showSupportedMods;
            }
        }

        if (_showSupportedMods)
        {
            foreach (var mod in SpaceWarpManager.SpaceWarpPlugins)
            {
                var style = SpaceWarpManager.ModsUnsupported[mod.SpaceWarpMetadata.ModID] ? _unsupportedModStyle
                    : SpaceWarpManager.ModsOutdated[mod.SpaceWarpMetadata.ModID] ? _outdatedModStyle
                    : null;

                DrawModListItem(mod.Info.Metadata.GUID, mod.SpaceWarpMetadata.Name, () =>
                {
                    _selectedBepIn = false;
                    _selectedMetaData = mod.SpaceWarpMetadata;
                }, style);
            }
        }

        GUILayout.Label("");
        if (_showUnmanagedMods)
        {
            if (GUILayout.Button($"{_unmanagedMods} ▼", _headerStyle))
            {
                _showUnmanagedMods = !_showUnmanagedMods;
            }
        }
        else
        {
            if (GUILayout.Button($"{_unmanagedMods} ▲", _headerStyle))
            {
                _showUnmanagedMods = !_showUnmanagedMods;
            }
        }

        if (_showUnmanagedMods)
        {
            foreach (var (plugin, info) in SpaceWarpManager.NonSpaceWarpInfos)
            {
                var style = SpaceWarpManager.ModsUnsupported[info.ModID] ? _unsupportedModStyle
                    : SpaceWarpManager.ModsOutdated[info.ModID] ? _outdatedModStyle
                    : null;

                DrawModListItem(plugin.Info.Metadata.GUID, info.Name, () =>
                {
                    _selectedBepIn = false;
                    _selectedMetaData = info;
                }, style);
            }

            foreach (var mod in SpaceWarpManager.NonSpaceWarpPlugins)
            {
                DrawModListItem(mod.Info.Metadata.GUID, mod.Info.Metadata.Name, () =>
                {
                    _selectedBepIn = true;
                    _selectedBepInMetadata = mod.Info.Metadata;
                });
            }
        }

        GUILayout.Label("");
        if (_showDisabledMods)
        {
            if (GUILayout.Button($"{_disabledMods} ▼", _headerStyle))
            {
                _showDisabledMods = !_showDisabledMods;
            }
        }
        else
        {
            if (GUILayout.Button($"{_disabledMods} ▲", _headerStyle))
            {
                _showDisabledMods = !_showDisabledMods;
            }
        }

        if (_showDisabledMods)
        {
            foreach (var (plugin, info) in SpaceWarpManager.DisabledInfoPlugins)
            {
                var style = SpaceWarpManager.ModsUnsupported[info.ModID] ? _unsupportedModStyle
                    : SpaceWarpManager.ModsOutdated[info.ModID] ? _outdatedModStyle
                    : null;

                DrawModListItem(plugin.Metadata.GUID, info.Name, () =>
                {
                    _selectedBepIn = false;
                    _selectedMetaData = info;
                }, style);
            }

            foreach (var plugin in SpaceWarpManager.DisabledNonInfoPlugins)
            {
                DrawModListItem(plugin.Metadata.GUID, plugin.Metadata.Name, () =>
                {
                    _selectedBepIn = true;
                    _selectedBepInMetadata = plugin.Metadata;
                });
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        if (_selectedBepIn)
        {
            if (_selectedBepInMetadata != null)
            {
                GUILayout.BeginVertical();
                _scrollPositionInfo = GUILayout.BeginScrollView(_scrollPositionInfo, false, false);
                GUILayout.Label($"{_selectedBepInMetadata.Name} (guid: {_selectedBepInMetadata.GUID})");
                GUILayout.Label($"{_version}: {_selectedBepInMetadata.Version}");
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
        }
        else
        {
            if (_selectedMetaData != null)
            {
                GUILayout.BeginVertical();
                _scrollPositionInfo = GUILayout.BeginScrollView(_scrollPositionInfo, false, false);
                GUILayout.Label($"{_selectedMetaData.Name} (id: {_selectedMetaData.ModID})");
                GUILayout.Label($"{_author}: {_selectedMetaData.Author}");
                GUILayout.Label(SpaceWarpManager.ModsOutdated[_selectedMetaData.ModID]
                    ? $"{_version}: {_selectedMetaData.Version} ({_outdated})"
                    : $"{_version}: {_selectedMetaData.Version}");
                GUILayout.Label($"Source: {_selectedMetaData.Source}");
                GUILayout.Label($"Description: {_selectedMetaData.Description}");
                GUILayout.Label(SpaceWarpManager.ModsUnsupported[_selectedMetaData.ModID]
                    ? $"{_ksp2Version}: {_selectedMetaData.SupportedKsp2Versions.Min} - {_selectedMetaData.SupportedKsp2Versions.Max} ({_unsupported})"
                    : $"{_ksp2Version}: {_selectedMetaData.SupportedKsp2Versions.Min} - {_selectedMetaData.SupportedKsp2Versions.Max}");
                GUILayout.Label(_dependencies);

                foreach (var dependency in _selectedMetaData.Dependencies)
                    GUILayout.Label($"{dependency.ID}: {dependency.Version.Min} - {dependency.Version.Max}");

                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
        }

        GUILayout.EndHorizontal();
        if (GUILayout.Button(_openConfigManager))
        {
            SpaceWarpManager.ConfigurationManager.DisplayingWindow =
                !SpaceWarpManager.ConfigurationManager.DisplayingWindow;
            _drawUI = false;
        }

        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    private void DrawModListItem(string GUID, string modName, Action onSelected, GUIStyle style = null)
    {
        int toggleIndex = _toggles.FindIndex(t => t.Item1 == GUID);
        bool isToggled = _toggles[toggleIndex].Item2; // current state of the toggle
        bool wasToggled = _wasToggledDict.ContainsKey(GUID) && _wasToggledDict[GUID]; // previous state of the toggle (defaults to false if not found)

        GUILayout.BeginHorizontal();
        _toggles[toggleIndex] = (GUID, GUILayout.Toggle(isToggled, ""));

        var buttonName = Trim(modName);
        if (style == null ? GUILayout.Button(buttonName) : GUILayout.Button(buttonName, style))
        {
            onSelected();
        }
        GUILayout.EndHorizontal();
        
        // Edge detection
        if (!isToggled && wasToggled) // falling edge
        {
            UpdateDisabledFile();
        }
        else if (isToggled && !wasToggled) // rising edge
        {
            UpdateDisabledFile();
        }

        _wasToggledDict[GUID] = isToggled; // update the previous state of the toggle
    }

    private void UpdateDisabledFile()
    {
        File.WriteAllLines(
            ChainloaderPatch.DisabledPluginsFilepath,
            _toggles.Where(item => !item.Item2).Select(item => item.Item1)
        );
    }

    public void ToggleVisible()
    {
        _drawUI = !_drawUI;
    }

    public void CloseWindow()
    {
        ToggleVisible();
    }
}