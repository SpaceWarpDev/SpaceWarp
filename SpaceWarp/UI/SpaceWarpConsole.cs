using System;
using System.Collections.Generic;
using KSP.Game;
using KSP.Sim.impl;
using SpaceWarp.API;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Managers;
using UnityEngine;

using KSP.Logging;
namespace SpaceWarp.UI
{
    public class SpaceWarpConsole : KerbalBehavior
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
            KspLogManager.AddLogCallback(LogCallback);
            windowWidth = (int)(Screen.width * 0.5f);
            windowHeight = (int)(Screen.height * 0.5f);
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
                    DrawConsole,
                    "Space Warp Console",
                    GUILayout.Width((float)(windowWidth * 0.8)),
                    GUILayout.Height((float)(windowHeight * 0.8))
                );
            }
        }

        private static GUIStyle boxStyle;
        private static Vector2 scrollPosition;

        private List<string> debugMessages = new List<string>();

        private void LogCallback(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    debugMessages.Add($"[ERR] {condition}");
                    break;
                case LogType.Assert:
                    debugMessages.Add($"[AST] {condition}");
                    break;
                case LogType.Warning:
                    debugMessages.Add($"[WRN] {condition}");
                    break;
                case LogType.Log:
                    debugMessages.Add($"[LOG] {condition}");
                    break;
                case LogType.Exception:
                    debugMessages.Add($"[EXC] {condition}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C))
            {
                drawUI = !drawUI;
            }
        }

        private void DrawConsole(int windowID)
        {
            boxStyle = GUI.skin.GetStyle("Box");
            GUILayout.BeginVertical();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
            foreach (var debugMessage in debugMessages)
            {
                GUILayout.Label(debugMessage);
            }
            
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close"))
            {
                drawUI = false;
            }

            if (GUILayout.Button("Clear"))
            {
                debugMessages.Clear();
            }

            if (GUILayout.Button("Clear Control Locks"))
            {
                KSP.Game.GameManager.Instance.Game.ViewController.inputLockManager.ClearControlLocks();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 500));
        }
    }
}