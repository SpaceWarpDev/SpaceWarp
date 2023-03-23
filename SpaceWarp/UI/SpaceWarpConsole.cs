using BepInEx.Bootstrap;
using BepInEx.Logging;
using I2.Loc;
using KSP.Game;
using KSP.UI.Binding;
using SpaceWarp.API.Assets;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;

namespace SpaceWarp.UI;

public sealed class SpaceWarpConsole : KerbalMonoBehaviour
{
    private const ControlTypes ConsoleLocks = ControlTypes.All;
    private const string ConsoleLockID = "spacewarp.console";

    private static Vector2 _scrollPosition;
    private static Vector2 _scrollView;

    private bool _autoScroll = true;

    private GUIStyle _closeButtonStyle;
    private bool _drawUI;
    private string _search = "";
    private SpaceWarpPlugin _spaceWarpPluginInstance;
    private int _windowHeight = 700;
    private Rect _windowRect;

    private int _windowWidth = 350;

    private static LocalizedString _header = "SpaceWarp/Console/Header";
    private static LocalizedString _clear = "SpaceWarp/Console/Clear";
    private static LocalizedString _autoScrollText = "SpaceWarp/Console/AutoScroll";
    private static LocalizedString _on = "SpaceWarp/Console/On";
    private static LocalizedString _off = "SpaceWarp/Console/Off";


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
        _scrollPosition = Vector2.zero;
        _spaceWarpPluginInstance = (Chainloader.PluginInfos[SpaceWarpPlugin.ModGuid].Instance as SpaceWarpPlugin)!;

        if (_spaceWarpPluginInstance.ConfigShowConsoleButton.Value)
            Appbar.RegisterAppButton(
                "Console",
                "BTN-SWConsole",
                AssetManager.GetAsset<Texture2D>("spacewarp/images/console.png"),
                ToggleVisible
            );
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C)) ToggleVisible(!_drawUI);

        if (Input.GetKey(KeyCode.Escape) && _drawUI)
        {
            CloseWindow();
            GUIUtility.ExitGUI();
        }

        if (Input.mouseScrollDelta.y + Input.GetAxis("Vertical") > 0) _autoScroll = false;
    }

    private void OnGUI()
    {
        GUI.skin = SpaceWarpManager.Skin;

        if (!_drawUI) return;

        _closeButtonStyle ??= new GUIStyle(GUI.skin.button)
        {
            fontSize = 8
        };

        var controlID = GUIUtility.GetControlID(FocusType.Passive);
        var width = GUILayout.Width((float)(_windowWidth * 0.8));
        var height = GUILayout.Height((float)(_windowHeight * 0.8));

        _windowRect = GUILayout.Window(controlID, _windowRect, DrawConsole, _header, width, height);
    }

    private void DrawConsole(int windowID)
    {
        if (GUI.Button(new Rect(_windowRect.width - 18, 2, 16, 16), "x", _closeButtonStyle))
        {
            CloseWindow();
            GUIUtility.ExitGUI();
        }

        GUILayout.BeginVertical();
        _search = GUILayout.TextField(_search);
        _scrollView = GUILayout.BeginScrollView(_scrollPosition, false, true);

        foreach (var message in SpaceWarpConsoleLogListener.DebugMessages)
        {
            if (!message.ToLower().Contains(_search.ToLower())) continue;

            // Parse the log level from the message string
            var logType = GetLogLevelFromMessage(message);

            // Apply a different color style based on the log level
            var style = GetLogStyle(logType);

            if (logType == LogLevel.Fatal) style.fontStyle = FontStyle.Bold;

            GUILayout.Label(message, style);
        }

        if (_autoScroll)
        {
            _scrollView.Set(_scrollView.x, Mathf.Infinity);
            _scrollPosition = _scrollView;
        }
        else
        {
            _scrollPosition = _scrollView;
        }

        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(_clear)) SpaceWarpConsoleLogListener.DebugMessages.Clear();

        if (GUILayout.Button(_autoScroll ? $"{_autoScrollText}: {_on}" : $"{_autoScrollText}: {_off}"))
            //Todo: Add proper close button to top corner and add input lock button back. 
            // GameManager.Instance.Game.ViewController.inputLockManager.ClearControlLocks();
            _autoScroll = !_autoScroll;

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 500));
    }

    private LogLevel GetLogLevelFromMessage(string message)
    {
        var logParts = message.ToLower().Replace(" ", "").Split(']');
        var logLevelIndex = _spaceWarpPluginInstance.ConfigShowTimeStamps.Value ? 1 : 0;

        return logParts[logLevelIndex] switch
        {
            { } logMessage when logMessage.StartsWith("[fatal") => LogLevel.Fatal,
            { } logMessage when logMessage.StartsWith("[error") => LogLevel.Error,
            { } logMessage when logMessage.StartsWith("[warn") => LogLevel.Warning,
            { } logMessage when logMessage.StartsWith("[message") => LogLevel.Message,
            { } logMessage when logMessage.StartsWith("[info") => LogLevel.Info,
            { } logMessage when logMessage.StartsWith("[debug") => LogLevel.Debug,
            { } logMessage when logMessage.StartsWith("[all") => LogLevel.All,
            _ => LogLevel.None
        };
    }

    private GUIStyle GetLogStyle(LogLevel logLevel)
    {
        var style = new GUIStyle(GUI.skin.label);

        style.normal.textColor = logLevel switch
        {
            LogLevel.Fatal => _spaceWarpPluginInstance.ConfigErrorColor.Value,
            LogLevel.Error => _spaceWarpPluginInstance.ConfigErrorColor.Value,
            LogLevel.Warning => _spaceWarpPluginInstance.ConfigWarningColor.Value,
            LogLevel.Message => _spaceWarpPluginInstance.ConfigMessageColor.Value,
            LogLevel.Info => _spaceWarpPluginInstance.ConfigInfoColor.Value,
            LogLevel.Debug => _spaceWarpPluginInstance.ConfigDebugColor.Value,
            LogLevel.All => _spaceWarpPluginInstance.ConfigAllColor.Value,
            _ => style.normal.textColor
        };

        return style;
    }

    public void ToggleVisible(bool shouldDraw)
    {
        _drawUI = shouldDraw;
        Game.ViewController.inputLockManager.SetControlLock(_drawUI ? ConsoleLocks : ControlTypes.None, ConsoleLockID);
    }

    public void CloseWindow()
    {
        ToggleVisible(false);
        GameObject.Find("BTN-SWConsole")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(false);
    }
}