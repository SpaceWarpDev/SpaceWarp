using System;
using KSP.Game;
using SpaceWarp.API;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Managers;
using UnityEngine;

namespace SpaceWarp.UI
{
    
    public class ModListUI : KerbalMonoBehaviour
    {
        static bool loaded = false;
        private bool drawUI = false;
        private Rect windowRect;
        private int windowWidth = 350;
        private int windowHeight = 700;
        public void Start()
        {
            if (loaded)
            {
                Destroy(this);
            }

            loaded = true;
        }

        void Awake()
        {
            windowWidth = (int)(Screen.width * 0.85f);
            windowHeight = (int)(Screen.height * 0.85f);
            windowRect = new Rect((Screen.width * 0.15f), (Screen.height * 0.15f),
                0, 0);
        }

        private void OnGUI()
        {
            if (drawUI)
            {
                windowRect = GUILayout.Window(
                    GUIUtility.GetControlID(FocusType.Passive),
                    windowRect,
                    FillWindow,
                    "Space Warp Mod List",
                    GUILayout.Width((float)(windowWidth * 0.8)),
                    GUILayout.Height((float)(windowHeight * 0.8))
                );
            }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.M))
            {
                drawUI = !drawUI;
            }
        }

        private static GUIStyle boxStyle;
        private static Vector2 scrollPositionMods;
        private string selectedMod;
        private ModInfo selectedModInfo;

        private void FillWindow(int windowID)
        {
            boxStyle = GUI.skin.GetStyle("Box");
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            scrollPositionMods = GUILayout.BeginScrollView(scrollPositionMods, false, true,
                GUILayout.Height((float)(windowHeight * 0.8)), GUILayout.Width(300));
            SpaceWarpManager manager;
            if (ManagerLocator.TryGet(out manager))
            {
                foreach ((string modID, ModInfo modInfo) in manager.LoadedMods)
                {
                    if (GUILayout.Button(modID))
                    {
                        selectedMod = modID;
                        selectedModInfo = modInfo;
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            if (selectedModInfo != null)
            {
                GUILayout.Label(selectedModInfo.name);
                GUILayout.Label($"Author: {selectedModInfo.author}");
                GUILayout.Label($"Version: {selectedModInfo.version}");
                GUILayout.Label($"Description: {selectedModInfo.description}");
                GUILayout.Label($"KSP2 Version: {selectedModInfo.supported_ksp2_versions.min} - {selectedModInfo.supported_ksp2_versions.max}");
                GUILayout.Label($"Dependencies");
                foreach (var dependency in selectedModInfo.dependencies)
                {
                    GUILayout.Label($"{dependency.id}: {dependency.version.min} - {dependency.version.max}");
                }
                
                if (ManagerLocator.TryGet(out ConfigurationManager configManager))
                {
                    if (configManager.TryGet(selectedModInfo.mod_id, out var config))
                    {
                        if (GUILayout.Button("Configure"))
                        {
                            GameObject go = new GameObject(selectedModInfo.mod_id);
                            go.transform.SetParent(transform);
                            ModConfigurationUI configUI = go.AddComponent<ModConfigurationUI>();
                            configUI.configurationType = config.configType;
                            configUI.configurationObject = config.configObject;
                            configUI.name = selectedModInfo.name;
                            configUI.modID = selectedModInfo.mod_id;
                            go.SetActive(true);
                        }
                    }
                }
            }
            else
            {
                GUILayout.Label("No mod selected");
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.DragWindow(new Rect(0, 0, 10000, 500));
        }
    }
}