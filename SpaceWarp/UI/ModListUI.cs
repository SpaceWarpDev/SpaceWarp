using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using SpaceWarpPatcher;
using I2.Loc;
using KSP.Game;
using SpaceWarp.API.Mods.JSON;
using UnityEngine;

namespace SpaceWarp.UI;

public class ModListUI : KerbalMonoBehaviour
{
    private static readonly LocalizedString EnableAll = "SpaceWarp/ModList/EnableAll";
    private static readonly LocalizedString DisableAll = "SpaceWarp/ModList/DisableAll";
    private static readonly LocalizedString RevertChanges = "SpaceWarp/ModList/RevertChanges";
    private static readonly LocalizedString ChangesDetected = "SpaceWarp/ModList/ChangesDetected";
    private static readonly LocalizedString ModListHeader = "SpaceWarp/ModList/Header";
    private static readonly LocalizedString SpaceWarpMods = "SpaceWarp/ModList/SpaceWarpMods";
    private static readonly LocalizedString UnmanagedMods = "SpaceWarp/ModList/UnmanagedMods";
    private static readonly LocalizedString DisabledMods = "SpaceWarp/ModList/DisabledMods";
    private static readonly LocalizedString Version = "SpaceWarp/ModList/Version";
    private static readonly LocalizedString Author = "SpaceWarp/ModList/Author";
    private static readonly LocalizedString Outdated = "SpaceWarp/ModList/outdated";
    private static readonly LocalizedString Source = "SpaceWarp/ModList/Source";
    private static readonly LocalizedString Description = "SpaceWarp/ModList/Description";
    private static readonly LocalizedString Ksp2Version = "SpaceWarp/ModList/KSP2Version";
    private static readonly LocalizedString Dependencies = "SpaceWarp/ModList/Dependencies";
    private static readonly LocalizedString OpenConfigManager = "SpaceWarp/ModList/OpenConfigManager";
    private static readonly LocalizedString Unsupported = "SpaceWarp/ModList/unsupported";
    
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
    private static GUIStyle _disabledModStyle;
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

    private static readonly IReadOnlyList<string> NoTogglePlugins = new List<string>
    {
        "com.github.x606.spacewarp",
        "com.bepis.bepinex.configurationmanager"
    };

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
        if (_loaded)
        {
            Destroy(this);
        }

        _loaded = true;
        _initialToggles = SpaceWarpManager.PluginGuidEnabledStatus.ToList().FindAll(
            item => !NoTogglePlugins.Contains(item.Item1)
        );
        _toggles = new List<(string, bool)>(_initialToggles);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.M))
        {
            ToggleVisible();
        }

        if (Input.GetKey(KeyCode.Escape) && _drawUI)
        {
            CloseWindow();
            GUIUtility.ExitGUI();
        }
    }

    private void OnGUI()
    {
        GUI.skin = SpaceWarpManager.Skin;
        
        if (!_drawUI)
        {
            return;
        }

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
        
        _disabledModStyle ??= new GUIStyle(GUI.skin.button)
        {
            normal =
            {
                textColor = Color.gray
            },
            active =
            {
                textColor = Color.gray
            },
            hover =
            {
                textColor = Color.gray
            },
            focused =
            {
                textColor = Color.gray
            },
            onActive =
            {
                textColor = Color.gray
            },
            onFocused =
            {
                textColor = Color.gray
            },
            onHover =
            {
                textColor = Color.gray
            },
            onNormal =
            {
                textColor = Color.gray
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

        _windowRect = GUILayout.Window(controlID, _windowRect, FillWindow, ModListHeader, width, height);
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
        if (GUILayout.Button(DisableAll))
        {
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i] = (_toggles[i].Item1, false);
            }
        }
        
        if (GUILayout.Button(EnableAll))
        {
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i] = (_toggles[i].Item1, true);
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(RevertChanges))
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
            GUILayout.Label(string.Format(ChangesDetected, numChanges));
        }

        
        if (_showSupportedMods)
        {
            if (GUILayout.Button($"{SpaceWarpMods} ▼", _headerStyle))
            {
                _showSupportedMods = !_showSupportedMods;
            }
        }
        else
        {
            if (GUILayout.Button($"{SpaceWarpMods} ▲", _headerStyle))
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
            if (GUILayout.Button($"{UnmanagedMods} ▼", _headerStyle))
            {
                _showUnmanagedMods = !_showUnmanagedMods;
            }
        }
        else
        {
            if (GUILayout.Button($"{UnmanagedMods} ▲", _headerStyle))
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
            if (GUILayout.Button($"{DisabledMods} ▼", _headerStyle))
            {
                _showDisabledMods = !_showDisabledMods;
            }
        }
        else
        {
            if (GUILayout.Button($"{DisabledMods} ▲", _headerStyle))
            {
                _showDisabledMods = !_showDisabledMods;
            }
        }

        if (_showDisabledMods)
        {
            foreach (var (plugin, info) in SpaceWarpManager.DisabledInfoPlugins)
            {
                DrawModListItem(plugin.Metadata.GUID, info.Name, () =>
                {
                    _selectedBepIn = false;
                    _selectedMetaData = info;
                }, _disabledModStyle);
            }

            foreach (var plugin in SpaceWarpManager.DisabledNonInfoPlugins)
            {
                DrawModListItem(plugin.Metadata.GUID, plugin.Metadata.Name, () =>
                {
                    _selectedBepIn = true;
                    _selectedBepInMetadata = plugin.Metadata;
                }, _disabledModStyle);
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
                GUILayout.Label($"{Version}: {_selectedBepInMetadata.Version}");
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
                GUILayout.Label($"{Author}: {_selectedMetaData.Author}");
                GUILayout.Label(SpaceWarpManager.ModsOutdated[_selectedMetaData.ModID]
                    ? $"{Version}: {_selectedMetaData.Version} ({Outdated})"
                    : $"{Version}: {_selectedMetaData.Version}");
                GUILayout.Label($"Source: {_selectedMetaData.Source}");
                GUILayout.Label($"Description: {_selectedMetaData.Description}");
                GUILayout.Label(SpaceWarpManager.ModsUnsupported[_selectedMetaData.ModID]
                    ? $"{Ksp2Version}: {_selectedMetaData.SupportedKsp2Versions.Min} - {_selectedMetaData.SupportedKsp2Versions.Max} ({Unsupported})"
                    : $"{Ksp2Version}: {_selectedMetaData.SupportedKsp2Versions.Min} - {_selectedMetaData.SupportedKsp2Versions.Max}");
                GUILayout.Label(Dependencies);

                foreach (var dependency in _selectedMetaData.Dependencies)
                {
                    GUILayout.Label($"{dependency.ID}: {dependency.Version.Min} - {dependency.Version.Max}");
                }

                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
        }

        GUILayout.EndHorizontal();
        if (GUILayout.Button(OpenConfigManager))
        {
            SpaceWarpManager.ConfigurationManager.DisplayingWindow = !SpaceWarpManager.ConfigurationManager.DisplayingWindow;
            _drawUI = false;
        }

        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    private void DrawModListItem(string guid, string modName, Action onSelected, GUIStyle style = null)
    {
        bool isToggled = false;
        bool wasToggled = false;

        GUILayout.BeginHorizontal();
        if (!NoTogglePlugins.Contains(guid))
        {
            int toggleIndex = _toggles.FindIndex(t => t.Item1 == guid);
            isToggled = _toggles[toggleIndex].Item2; // current state of the toggle
            wasToggled = _wasToggledDict.ContainsKey(guid) && _wasToggledDict[guid]; // previous state of the toggle (defaults to false if not found)
            
            _toggles[toggleIndex] = (guid, GUILayout.Toggle(isToggled, ""));
        }
        else
        {
            GUILayout.Space(25);
        }

        var buttonName = Trim(modName);
        if (style == null ? GUILayout.Button(buttonName) : GUILayout.Button(buttonName, style))
        {
            onSelected();
        }
        GUILayout.EndHorizontal();
        
        if ((!isToggled && wasToggled) || (isToggled && !wasToggled))
        {
            UpdateDisabledFile();
        }

        _wasToggledDict[guid] = isToggled; // update the previous state of the toggle
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