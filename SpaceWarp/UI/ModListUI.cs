using BepInEx;
using KSP.Game;
using SpaceWarp.API.Mods.JSON;
using UnityEngine;

namespace SpaceWarp.UI;

public class ModListUI : KerbalMonoBehaviour
{
    private const string ModListHeader = "spacewarp.modlist";
    private static bool _loaded;

    private static Vector2 _scrollPositionMods;
    private static Vector2 _scrollPositionInfo;
    private static GUIStyle _closeButtonStyle;
    private static GUIStyle _outdatedModStyle;
    private static GUIStyle _unsupportedModStyle;
    private static GUIStyle _unmanagedHeaderStyle;

    private bool _drawUI;
    private bool _selectedBepIn;
    private BepInPlugin _selectedBepInMetadata;
    private ModInfo _selectedMetaData;
    private int _windowHeight = 700;
    private Rect _windowRect;

    private int _windowWidth = 350;

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
        _unmanagedHeaderStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        var controlID = GUIUtility.GetControlID(FocusType.Passive);
        var width = GUILayout.Width((float)(_windowWidth * 0.8));
        var height = GUILayout.Height((float)(_windowHeight * 0.8));

        _windowRect = GUILayout.Window(controlID, _windowRect, FillWindow, ModListHeader, width, height);
    }

    private void FillWindow(int windowID)
    {
        GUI.skin.GetStyle("Box");
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

        GUILayout.Label("SpaceWarp Mods", _unmanagedHeaderStyle);

        foreach (var mod in SpaceWarpManager.SpaceWarpPlugins)
            if (SpaceWarpManager.ModsUnsupported[mod.SpaceWarpMetadata.ModID])
            {
                if (!GUILayout.Button(mod.SpaceWarpMetadata.Name, _unsupportedModStyle)) continue;
                _selectedBepIn = false;
                _selectedMetaData = mod.SpaceWarpMetadata;
            }
            else if (SpaceWarpManager.ModsOutdated[mod.SpaceWarpMetadata.ModID])
            {
                if (!GUILayout.Button(mod.SpaceWarpMetadata.Name, _outdatedModStyle)) continue;
                _selectedBepIn = false;
                _selectedMetaData = mod.SpaceWarpMetadata;
            }
            else
            {
                if (!GUILayout.Button(mod.SpaceWarpMetadata.Name)) continue;
                _selectedBepIn = false;
                _selectedMetaData = mod.SpaceWarpMetadata;
            }

        GUILayout.Label("");
        GUILayout.Label("Unmanaged Mods", _unmanagedHeaderStyle);

        foreach (var info in SpaceWarpManager.NonSpaceWarpInfos)
            if (SpaceWarpManager.ModsUnsupported[info.ModID])
            {
                if (!GUILayout.Button(info.Name, _unsupportedModStyle)) continue;
                _selectedBepIn = false;
                _selectedMetaData = info;
            }
            else if (SpaceWarpManager.ModsOutdated[info.ModID])
            {
                if (!GUILayout.Button(info.Name, _outdatedModStyle)) continue;
                _selectedBepIn = false;
                _selectedMetaData = info;
            }
            else
            {
                if (!GUILayout.Button(info.Name)) continue;
                _selectedBepIn = false;
                _selectedMetaData = info;
            }

        foreach (var mod in SpaceWarpManager.NonSpaceWarpPlugins)
        {
            if (!GUILayout.Button(mod.Info.Metadata.Name)) continue;
            _selectedBepIn = true;
            _selectedBepInMetadata = mod.Info.Metadata;
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
                GUILayout.Label($"Version: {_selectedBepInMetadata.Version}");
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
                GUILayout.Label($"Author: {_selectedMetaData.Author}");
                GUILayout.Label(SpaceWarpManager.ModsOutdated[_selectedMetaData.ModID]
                    ? $"Version: {_selectedMetaData.Version} (outdated)"
                    : $"Version: {_selectedMetaData.Version}");
                GUILayout.Label($"Source: {_selectedMetaData.Source}");
                GUILayout.Label($"Description: {_selectedMetaData.Description}");
                GUILayout.Label(SpaceWarpManager.ModsUnsupported[_selectedMetaData.ModID]
                    ? $"KSP2 Version: {_selectedMetaData.SupportedKsp2Versions.Min} - {_selectedMetaData.SupportedKsp2Versions.Max} (unsupported)"
                    : $"KSP2 Version: {_selectedMetaData.SupportedKsp2Versions.Min} - {_selectedMetaData.SupportedKsp2Versions.Max}");
                GUILayout.Label("Dependencies");

                foreach (var dependency in _selectedMetaData.Dependencies)
                    GUILayout.Label($"{dependency.ID}: {dependency.Version.Min} - {dependency.Version.Max}");

                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
        }

        GUILayout.EndHorizontal();
        if (GUILayout.Button("Open Configuration Manager"))
        {
            SpaceWarpManager.ConfigurationManager.DisplayingWindow =
                !SpaceWarpManager.ConfigurationManager.DisplayingWindow;
            _drawUI = false;
        }

        GUILayout.EndVertical();
        GUI.DragWindow();
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