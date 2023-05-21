using I2.Loc;
using SpaceWarp.API.UI;
using UnityEngine;

namespace SpaceWarp.UI;

internal class VersionCheckPrompt : MonoBehaviour
{
    public SpaceWarpPlugin spaceWarpPlugin;
    private GUIStyle _closeButtonStyle;
    private float _windowHeight;
    private Rect _windowRect;
    private float _windowWidth;

    private static readonly LocalizedString SpaceWarp = "SpaceWarp";
    private static readonly LocalizedString VersionChecking = "SpaceWarp/VersionChecking";
    private static readonly LocalizedString Yes = "SpaceWarp/Yes";
    private static readonly LocalizedString No = "SpaceWarp/No";

    private void Awake()
    {
        const float minResolution = 1280f / 720f;
        const float maxResolution = 2048f / 1080f;
        var screenRatio = Screen.width / (float)Screen.height;
        var scaleFactor = Mathf.Clamp(screenRatio, minResolution, maxResolution);

        _windowWidth = (int)(Screen.width * 0.2f * scaleFactor);
        _windowHeight = 0;
        _windowRect = new Rect(
            Screen.width * 0.15f,
            Screen.height * 0.15f,
            Screen.width * 0.5f * scaleFactor,
            Screen.height * 0.5f * scaleFactor
        );
    }

    public void OnGUI()
    {
        GUI.skin = Skins.ConsoleSkin;

        var controlID = GUIUtility.GetControlID(FocusType.Passive);
        var width = GUILayout.Width((float)(_windowWidth * 0.8));
        var height = GUILayout.Height((float)(_windowHeight * 0.8));

        _closeButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            fontSize = 8
        };
        _windowRect = GUILayout.Window(controlID, _windowRect, FillWindow, SpaceWarp, width, height);
    }

    private void FillWindow(int windowID)
    {
        if (GUI.Button(new Rect(_windowRect.width - 18, 2, 16, 16), "x", _closeButtonStyle))
        {
            Destroy(this);
            GUIUtility.ExitGUI();
        }

        GUILayout.BeginVertical();
        GUILayout.Label(VersionChecking);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(Yes))
        {
            spaceWarpPlugin.CheckVersions();
            spaceWarpPlugin.ConfigCheckVersions.Value = true;
            Destroy(this);
        }

        if (GUILayout.Button(No))
        {
            Destroy(this);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.DragWindow();
    }
}