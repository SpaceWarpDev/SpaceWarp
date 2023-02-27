using System;
using KSP.Game;
using SpaceWarp.API;
using SpaceWarp.API.AssetBundles;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Managers;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.AssetBundles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpaceWarp.UI
{
    public class ModListUI : KerbalMonoBehaviour
    {
        private static bool _loaded;
        private bool _drawUI;
        private Rect _windowRect;

        private int _windowWidth = 350;
        private int _windowHeight = 700;
        public GUISkin _spaceWarpUISkin;

        private static GUIStyle _boxStyle;
        private static Vector2 _scrollPositionMods;
        private string _selectedMod;
        private ModInfo _selectedModInfo;
        private GUISkin _spaceWarpUISkin;

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

            _windowRect = new Rect((Screen.width * 0.15f), (Screen.height * 0.15f), 0, 0);
            ResourceManager.TryGetAsset($"space_warp/swconsoleui/spacewarpConsole.guiskin", out _spaceWarpUISkin);
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
            _boxStyle = GUI.skin.GetStyle("Box");
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            _scrollPositionMods = GUILayout.BeginScrollView(_scrollPositionMods, false, true, GUILayout.Height((float)(_windowHeight * 0.8)), GUILayout.Width(300));

            if (ManagerLocator.TryGet(out SpaceWarpManager manager))
            {
                foreach ((string modID, ModInfo modInfo) in manager.LoadedMods)
                {
                    if (GUILayout.Button(modID))
                    {
                        _selectedMod = modID;
                        _selectedModInfo = modInfo;
                    }
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
            GUI.DragWindow(new Rect(0, 0, 10000, 500));
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
}
