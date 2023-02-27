using System;
using System.Collections.Generic;
using KSP.Game;
using KSP.Sim.impl;
using UnityEngine;
using SpaceWarp.API.AssetBundles;
using KSP.Logging;
using static KSP.Map.impl.Targeting.Sample;
using static RTG.Object2ObjectSnap;
using UnityEngine.UI;
using BepInEx.Logging;

namespace SpaceWarp.UI
{
    public class SpaceWarpConsole : KerbalBehavior
    {
        private static bool _loaded;

        private bool _drawUI;
        private Rect _windowRect;

        private int _windowWidth = 350;
        private int _windowHeight = 700;

        private static GUIStyle _boxStyle;
        private static Vector2 _scrollPosition;

        public GUISkin _spaceWarpUISkin;

        private readonly List<string> _debugMessages = new List<string>();

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
            KspLogManager.AddLogCallback(LogCallback);

            _windowWidth = (int)(Screen.width * 0.5f);
            _windowHeight = (int)(Screen.height * 0.5f);
            _windowRect = new Rect((Screen.width * 0.15f), (Screen.height * 0.15f),0, 0);

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
            string header = $"spacewarp.console";
            GUILayoutOption width = GUILayout.Width((float)(_windowWidth * 0.8));
            GUILayoutOption height = GUILayout.Height((float)(_windowHeight * 0.8));

            _windowRect = GUILayout.Window(controlID, _windowRect, DrawConsole, header, width, height);
        }

        private void LogCallback(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    _debugMessages.Add($"[ERR] {condition}");
                    break;
                case LogType.Assert:
                    _debugMessages.Add($"[AST] {condition}");
                    break;
                case LogType.Warning:
                    _debugMessages.Add($"[WRN] {condition}");
                    break;
                case LogType.Log:
                    _debugMessages.Add($"[LOG] {condition}");
                    break;
                case LogType.Exception:
                    _debugMessages.Add($"[EXC] {condition}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
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
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

            foreach (string debugMessage in _debugMessages)
            {
                GUILayout.Label(debugMessage);
            }

            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Close"))
            {
                _drawUI = false;
            }

            if (GUILayout.Button("Clear"))
            {
                SpaceWarpConsoleLogListener.DebugMessages.Clear();
            }

            if (GUILayout.Button("Clear Control Locks"))
            {
                GameManager.Instance.Game.ViewController.inputLockManager.ClearControlLocks();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 500));
        }
    }
}