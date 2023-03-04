using System;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using KSP.Game;
using KSP.UI.Binding;
using SpaceWarp.API.Assets;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;

namespace SpaceWarp.UI;

public sealed class SpaceWarpConsole : KerbalMonoBehaviour
{
    private bool _drawUI;
    private Rect _windowRect;
    private string _search = "";
    private bool _autoScroll = true;

    private const ControlTypes ConsoleLocks = ControlTypes.All;
    private const string ConsoleLockID = "spacewarp.console";

    private int _windowWidth = 350;
    private int _windowHeight = 700;

    private static Vector2 _scrollPosition;
    private static Vector2 _scrollView;

    private GUIStyle _closeButtonStyle;
    private SpaceWarpPlugin _spaceWarpPluginInstance;


    private void Awake()
    {
        _windowWidth = (int)(Screen.width * 0.5f);
        _windowHeight = (int)(Screen.height * 0.5f);
        _windowRect = new Rect(Screen.width * 0.15f, Screen.height * 0.15f, 0, 0);
        _scrollPosition = Vector2.zero;
        _spaceWarpPluginInstance = (Chainloader.PluginInfos[SpaceWarpPlugin.ModGuid].Instance as SpaceWarpPlugin)!;

        if (_spaceWarpPluginInstance.configShowConsoleButton.Value)
            Appbar.RegisterAppButton(
                "Console",
                "BTN-SWConsole",
                // Example of using the asset loader, were going to load the apps icon
                // Path format [mod_id]/images/filename
                // for bundles its [mod_id]/[bundle_name]/[path to file in bundle with out assets/bundle]/filename.extension
                // There is also a try get asset function, that returns a bool on whether or not it could grab the asset
                AssetManager.GetAsset<Texture2D>("spacewarp/images/console.png"),
                ToggleVisible
            );
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

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        GUILayoutOption width = GUILayout.Width((float)(_windowWidth * 0.8));
        GUILayoutOption height = GUILayout.Height((float)(_windowHeight * 0.8));

        _windowRect = GUILayout.Window(controlID, _windowRect, DrawConsole, ConsoleLockID, width, height);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C))
        {
            ToggleVisible(!_drawUI);
        }

        if (Input.GetKey(KeyCode.Escape) && _drawUI)
        {
            CloseWindow();
            GUIUtility.ExitGUI();
        }
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

        foreach (string message in SpaceWarpConsoleLogListener.DebugMessages)
        {
            if (!message.ToLower().Contains(_search.ToLower())) continue;

            // Parse the log level from the message string
            LogLevel logType = GetLogLevelFromMessage(message);

            // Apply a different color style based on the log level
            GUIStyle style = GetLogStyle(logType);

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

        if (GUILayout.Button("Clear"))
        {
            SpaceWarpConsoleLogListener.DebugMessages.Clear();
        }

        if (GUILayout.Button(_autoScroll ? "Auto Scroll: On" : "Auto Scroll: Off"))
        {
            //Todo: Add proper close button to top corner and add input lock button back. 
            // GameManager.Instance.Game.ViewController.inputLockManager.ClearControlLocks();
            _autoScroll = !_autoScroll;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 500));
    }

    private LogLevel GetLogLevelFromMessage(string message)
    {
        var logParts = message.ToLower().Replace(" ", "").Split(']');
        var logLevelIndex = _spaceWarpPluginInstance.configShowTimeStamps.Value ? 1 : 0;

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
        GUIStyle style = new GUIStyle(GUI.skin.label);

        style.normal.textColor = logLevel switch
        {
            LogLevel.Fatal => _spaceWarpPluginInstance.configErrorColor.Value,
            LogLevel.Error => _spaceWarpPluginInstance.configErrorColor.Value,
            LogLevel.Warning => _spaceWarpPluginInstance.configWarningColor.Value,
            LogLevel.Message => _spaceWarpPluginInstance.configMessageColor.Value,
            LogLevel.Info => _spaceWarpPluginInstance.configInfoColor.Value,
            LogLevel.Debug => _spaceWarpPluginInstance.configDebugColor.Value,
            LogLevel.All => _spaceWarpPluginInstance.configAllColor.Value,
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