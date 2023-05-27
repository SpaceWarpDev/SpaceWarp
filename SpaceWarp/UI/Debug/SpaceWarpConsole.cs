using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Bootstrap;	
using BepInEx.Logging;
using KSP.Animation;
using KSP.Game;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceWarp.UI.Debug;	

public sealed class SpaceWarpConsole : KerbalMonoBehaviour	
{	
    // State
    private bool _isLoaded;
    private bool _isWindowVisible;
    private bool _isAutoScrollEnabled = true;
    
    private SpaceWarpPlugin _spaceWarpPluginInstance;
    
    // UITK Stuff
    private VisualElement _container;

    private TextField _consoleInput;
    private ScrollView _consoleOutput;

    private Toggle _toggleError;
    private Toggle _toggleWarning;
    private Toggle _toggleFatal;
    private Toggle _toggleDebug;
    private Toggle _toggleMessage;
    private Toggle _toggleInfo;

    private bool _toggleErrorState;
    private bool _toggleWarningState;
    private bool _toggleFatalState;
    private bool _toggleDebugState;
    private bool _toggleMessageState;
    private bool _toggleInfoState;


    // Debugging
    private static ManualLogSource _logger;

    private void Start()
    {
        // Binds the OnNewMessageReceived function to the OnNewMessage event
        SpaceWarpConsoleLogListener.OnNewMessage += CreateNewLabel;
    }
    
    private void OnDestroy()
    {
        // Unbinds the OnNewMessageReceived function to the OnNewMessage event when destroyed
        SpaceWarpConsoleLogListener.OnNewMessage -= CreateNewLabel;
    }


    private void Awake()
    {
        if (_isLoaded) return;
        
        // Debugging
        _logger = SpaceWarpPlugin.Logger;
        
        // IDK what this does tbh but it fixed some issues
        _spaceWarpPluginInstance = (Chainloader.PluginInfos[SpaceWarpPlugin.ModGuid].Instance as SpaceWarpPlugin)!;	

        
        // Run the main UITK setup functions
        SetupDocument();
        InitializeElements();
        BindFunctions();
        SetDefaults();
        UnbindFunctions();

        _isLoaded = true;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C))   
        {
            ToggleWindow();
        }

        if (_isWindowVisible && Input.GetKey(KeyCode.Escape))
        {
            HideWindow();
        }
    }

    private void CreateNewLabel(string message)
    {
        var searchFilter = _consoleInput.value;
        int logLevel = GetLogLevelFromMessage(message);
        
        bool isMessageEnabled = IsLogLevelEnabled(logLevel);
        if (isMessageEnabled && !message.Contains(searchFilter))
        {
            return;
        }

        if (!isMessageEnabled)
        {
            return;
        }
        
        Label label = new Label(message)
        {
            style =
            {
                // Turns on word wrap
                whiteSpace = WhiteSpace.Normal
            }
        };

        // Parse the color from the message
        Color logLevelColor = GetColorFromLogLevel(logLevel);
        label.style.color = logLevelColor;
        
        // Add the label to the console output
        _consoleOutput.contentContainer.Add(label);
        
        // Scroll to the bottom of the console output
        if (_isAutoScrollEnabled)
        {
            AutoScrollToBottom();
        }
    }

    private void SetupDocument()
    {
        var document = GetComponent<UIDocument>();
        if (document.TryGetComponent<DocumentLocalization>(out var localization))
        {
            localization.Localize();
        }
        else
        {
            document.EnableLocalization();
        }
        
        _container = document.rootVisualElement;
        
        StartCoroutine(SetupWindow());
    }

    private IEnumerator SetupWindow()
    {
        yield return new WaitForFixedUpdate();

        // Centers the window
        var root = _container.hierarchy[0];
        root.transform.position = new Vector3(
            (Screen.width - root.boundingBox.width) / 2,
            (Screen.height - root.boundingBox.height) / 2
        );

        yield return new WaitForFixedUpdate();

        _container.style.display = DisplayStyle.None;
    }

    private void InitializeElements()
    {
        _consoleOutput = _container.Q<ScrollView>("console-output");
        _consoleInput = _container.Q<TextField>("console-input");

        // Binding all of the buttons to their respective functions
        _container.Q<Button>("back-button").RegisterCallback<ClickEvent>(_ => HideWindow());
        _container.Q<Button>("autoscroll-button").RegisterCallback<ClickEvent>(_ => ToggleAutoScroll());
        _container.Q<Button>("clear-button").RegisterCallback<ClickEvent>(_ => _consoleOutput.contentContainer.Clear());
        
        _toggleError = _container.Q<Toggle>("log-levels-toggle-error");
        _toggleWarning = _container.Q<Toggle>("log-levels-toggle-warning");
        _toggleFatal = _container.Q<Toggle>("log-levels-toggle-fatal");
        _toggleDebug = _container.Q<Toggle>("log-levels-toggle-debug");
        _toggleMessage = _container.Q<Toggle>("log-levels-toggle-message");
        _toggleInfo = _container.Q<Toggle>("log-levels-toggle-info");
    }
    
    private void filterHandler (ChangeEvent<bool> evt) => FilterMessages();
    private void searchHandler (ChangeEvent<string> evt) => FilterMessages();

    private void BindFunctions()
    {
        _consoleInput.RegisterValueChangedCallback(searchHandler);
        
        _toggleError.RegisterValueChangedCallback(filterHandler);
        _toggleWarning.RegisterValueChangedCallback(filterHandler);
        _toggleFatal.RegisterValueChangedCallback(filterHandler);
        _toggleDebug.RegisterValueChangedCallback(filterHandler);
        _toggleMessage.RegisterValueChangedCallback(filterHandler);
        _toggleInfo.RegisterValueChangedCallback(filterHandler);
    }

    private void UnbindFunctions()
    {
        _consoleOutput.UnregisterCallback<ChangeEvent<string>>(searchHandler);
        
        _toggleError.UnregisterValueChangedCallback(filterHandler);
        _toggleWarning.UnregisterValueChangedCallback(filterHandler);
        _toggleFatal.UnregisterValueChangedCallback(filterHandler);
        _toggleDebug.UnregisterValueChangedCallback(filterHandler);
        _toggleMessage.UnregisterValueChangedCallback(filterHandler);
        _toggleInfo.UnregisterValueChangedCallback(filterHandler);
    }

    private void SetDefaults()
    {
        _toggleInfo.value = true;
        _toggleMessage.value = true;
        _toggleDebug.value = true;
        _toggleFatal.value = true;
        _toggleWarning.value = true;
        _toggleError.value = true;
    }
    
    private void FilterMessages()
    {
        // Start off with the original messages.
        var filteredMessages = SpaceWarpConsoleLogListener.DebugMessages;
        var searchFilter = _consoleInput.value;
        
        // Get the states of each toggle in the form of a bool
        _toggleErrorState = _toggleError.value;
        _toggleWarningState = _toggleWarning.value;
        _toggleFatalState = _toggleFatal.value;
        _toggleDebugState = _toggleDebug.value;
        _toggleMessageState = _toggleMessage.value;
        _toggleInfoState = _toggleInfo.value;
        
        // Filter the messages based on the toggle states and use the start to determine the log level
        // I just rewrite the function to return a number for simpler code. Could have used the original colors
        // but meh.
        filteredMessages = filteredMessages.Where(message =>
        {
            var logLevelColor = GetLogLevelFromMessage(message);
            
            bool isMessageEnabled = IsLogLevelEnabled(logLevelColor);

            if (isMessageEnabled && !string.IsNullOrWhiteSpace(searchFilter) && !message.Contains(searchFilter))
            {
                isMessageEnabled = false;
            }

            return isMessageEnabled;
        }).ToList();
        
        
        // Update the console output to display the filtered messages
        UpdateConsoleOutput(filteredMessages);
    }
    
    // This is just used for a total rewrite of the console output in cases like searching and filtering.
    // WARNING: This is a very expensive function
    private void UpdateConsoleOutput(List<string> filteredMessages)
    {
        // Clear the console output
        _consoleOutput.contentContainer.Clear();
        
        // Add the filtered messages to the console output
        foreach (var message in filteredMessages)
        {
            CreateNewLabel(message);
        }
    }

    private bool IsLogLevelEnabled(int logLevel)
    {
        switch (logLevel)
        {
            case 0: // Fatal
                return _toggleFatalState;
            case 1: // Error
                return _toggleErrorState;
            case 2: // Warning
                return _toggleWarningState;
            case 3: // Message
                return _toggleMessageState;
            case 4: // Info
                return _toggleInfoState;
            case 5: // Debug
                return _toggleDebugState;
            case 6: // All
                return true;
            default:
                // rather it be visible than not
                return true;
        }
    }
    
    private int GetLogLevelFromMessage(string message)
    {
        try
        {
            // Split the message into parts
            var logParts = message.ToLower().Replace(" ", "").Split(']');
            var logLevelIndex = _spaceWarpPluginInstance.ConfigShowTimeStamps.Value ? 1 : 0;
            
            if (logLevelIndex < logParts.Length)
            {
                var logMessage = logParts[logLevelIndex];

                if (logMessage.StartsWith("[fatal"))
                    return 0;
                if (logMessage.StartsWith("[error"))
                    return 1;
                if (logMessage.StartsWith("[warn"))
                    return 2;
                if (logMessage.StartsWith("[message"))
                    return 3;
                if (logMessage.StartsWith("[info"))
                    return 4;
                if (logMessage.StartsWith("[debug"))
                    return 5;
                if (logMessage.StartsWith("[all"))
                    return 6;
            }
        }
        catch (Exception ex)
        {
            return 3;
        }

        // Return a fallback color if any error occurred during parsing
        return 3;
    }

    private Color GetColorFromLogLevel(int loglevel)
    {
        switch (loglevel)
        {
            case 0: // Fatal
                return Color.red;
            case 1: // Error
                return _spaceWarpPluginInstance.ConfigErrorColor.Value;
            case 2: // Warning
                return _spaceWarpPluginInstance.ConfigWarningColor.Value;
            case 3: // Message
                return _spaceWarpPluginInstance.ConfigMessageColor.Value;
            case 4: // Info
                return _spaceWarpPluginInstance.ConfigInfoColor.Value;
            case 5: // Debug
                return _spaceWarpPluginInstance.ConfigDebugColor.Value;
            default:
                return _spaceWarpPluginInstance.ConfigMessageColor.Value;
        }
    }

    private void ToggleAutoScroll()
    {
        _isAutoScrollEnabled = !_isAutoScrollEnabled;
        _logger.LogInfo("AutoScroll is now " + (_isAutoScrollEnabled ? "enabled" : "disabled") + ".");
    }

    private void AutoScrollToBottom()
    {
        StartCoroutine(AutoScrollToBottomCoroutine());
    }
    
    private IEnumerator AutoScrollToBottomCoroutine()
    {
        yield return null;

        _consoleOutput.ScrollTo(_consoleOutput.contentContainer[_consoleOutput.contentContainer.childCount - 1]);
    }
    
    private void ToggleWindow()
    {
        _isWindowVisible = !_isWindowVisible;
    
        if (_isWindowVisible)
        {
            _container.style.display = DisplayStyle.Flex;
            BindFunctions();
            AutoScrollToBottom();
        }
        else
        {
            _container.style.display = DisplayStyle.None;
            UnbindFunctions();
        }
    }

    private void HideWindow()
    {
        _container.style.display = DisplayStyle.None;
        _isWindowVisible = false;
        UnbindFunctions();
    }

}