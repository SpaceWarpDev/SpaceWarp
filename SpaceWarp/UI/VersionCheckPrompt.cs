using System;
using UnityEngine;

namespace SpaceWarp.UI;

internal class VersionCheckPrompt : MonoBehaviour
{
    public SpaceWarpPlugin spaceWarpPlugin;
    private GUIStyle _closeButtonStyle;
    private Rect _windowRect;
    private float _windowWidth;
    private float _windowHeight;

    private void Awake()
    {
        float minResolution = 1280f / 720f; 
        float maxResolution = 2048f / 1080f;
        float screenRatio = (float) Screen.width / (float) Screen.height;
        float scaleFactor = Mathf.Clamp(screenRatio, minResolution, maxResolution);

        _windowWidth = (int) (Screen.width * 0.5f * scaleFactor);
        _windowHeight = (int) (Screen.height * 0.5f * scaleFactor);
        _windowRect = new Rect(
            Screen.width * 0.15f,
            Screen.height * 0.15f,
            Screen.width * 0.5f * scaleFactor,
            Screen.height * 0.5f * scaleFactor
        );
    }
    public void OnGUI()
    {
        GUI.skin = SpaceWarp.API.UI.Skins.ConsoleSkin;

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        GUILayoutOption width = GUILayout.Width((float)(_windowWidth * 0.8));
        GUILayoutOption height = GUILayout.Height((float)(_windowHeight * 0.8));

        _closeButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            fontSize = 8
        };
        _windowRect = GUILayout.Window(controlID, _windowRect, FillWindow, "Space Warp", width, height);
    }

    private void FillWindow(int windowID)
    {
        if (GUI.Button(new Rect(_windowRect.width - 18, 2, 16, 16), "x", _closeButtonStyle))
        {
            Destroy(this);
            GUIUtility.ExitGUI();
        }

        GUILayout.BeginVertical();
        GUILayout.Label("Allow Space Warp to check versions for mods");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Yes"))
        {
            spaceWarpPlugin.CheckVersions();
            spaceWarpPlugin.configCheckVersions.Value = true;
            Destroy(this);
        }

        if (GUILayout.Button("No"))
        {
            Destroy(this);
        }
        
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.DragWindow();
    }
}