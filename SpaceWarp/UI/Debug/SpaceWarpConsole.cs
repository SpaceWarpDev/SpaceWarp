using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Bootstrap;	
using BepInEx.Logging;
using KSP.Animation;
using KSP.Game;
using SpaceWarp.API.Assets;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using static SpaceWarp.UI.Debug.SpaceWarpConsoleLogListener;

namespace SpaceWarp.UI.Debug;	

public sealed class SpaceWarpConsole : KerbalMonoBehaviour	
{	
    // State
    private bool _isLoaded;
    private bool _isWindowVisible;
    
    private SpaceWarpPlugin _spaceWarpPluginInstance;
    
    private VisualTreeAsset _consoleTemplate;

    // UITK Stuff
    private VisualElement _container;

    private TextField _consoleSearch;
    private string searchFilter => _consoleSearch.text;
    private ScrollView _consoleContent;

    private Toggle _toggleDebug;
    private Toggle _toggleMessage;
    private Toggle _toggleInfo;
    private Toggle _toggleWarning;
    private Toggle _toggleError;
    private Toggle _toggleAutoScroll;


    // Debugging
    private static ManualLogSource _logger;

    private void Start()
    {
        // Binds the OnNewMessageReceived function to the OnNewMessage event
        SpaceWarpConsoleLogListener.OnNewLog += CreateNewLogEntry;
    }


    private void OnDestroy()
    {
        // Unbinds the OnNewMessageReceived function to the OnNewMessage event when destroyed
        SpaceWarpConsoleLogListener.OnNewLog -= CreateNewLogEntry;
    
    }

    private void CreateNewLogEntry(LogInfo logInfo)
    {
        LogEntry entry = new(logInfo);
        entry.TextColor = GetColorFromLogLevel(logInfo.Level);
        entry.RegisterCallback<ClickEvent>((evt) => entry.MessageGrouper.style.display = entry.MessageGrouper.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex);
        _consoleContent.Add(entry);
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
        _consoleContent = _container.Q<ScrollView>("log-entry-content");
        _consoleContent.Clear();
        _consoleSearch = _container.Q<TextField>("search-text-field");
        _consoleSearch.text = string.Empty;

        // Binding all of the buttons to their respective functions
        _container.Q<Button>("exit-button").RegisterCallback<ClickEvent>(_ => HideWindow());
        _container.Q<Button>("clear-button").RegisterCallback<ClickEvent>(_ => _consoleContent.contentContainer.Clear());
        
        _toggleError = _container.Q<Toggle>("toggle-error");
        _toggleWarning = _container.Q<Toggle>("toggle-warning");
        _toggleDebug = _container.Q<Toggle>("toggle-debug");
        _toggleMessage = _container.Q<Toggle>("toggle-message");
        _toggleInfo = _container.Q<Toggle>("toggle-info");
        _toggleAutoScroll = _container.Q<Toggle>("toggle-autoscroll");
    }
    
    private void filterHandler (ChangeEvent<bool> evt) => FilterMessages();
    private void searchHandler (ChangeEvent<string> evt) => FilterMessages();

    private void BindFunctions()
    {
        _consoleSearch.RegisterValueChangedCallback(searchHandler);
        
        _toggleError.RegisterValueChangedCallback(filterHandler);
        _toggleWarning.RegisterValueChangedCallback(filterHandler);
        _toggleDebug.RegisterValueChangedCallback(filterHandler);
        _toggleMessage.RegisterValueChangedCallback(filterHandler);
        _toggleInfo.RegisterValueChangedCallback(filterHandler);
        _toggleAutoScroll.RegisterValueChangedCallback(filterHandler);
    }

    private void UnbindFunctions()
    {
        _consoleSearch.RegisterValueChangedCallback(searchHandler);
        
        _toggleError.UnregisterValueChangedCallback(filterHandler);
        _toggleWarning.UnregisterValueChangedCallback(filterHandler);
        _toggleDebug.UnregisterValueChangedCallback(filterHandler);
        _toggleMessage.UnregisterValueChangedCallback(filterHandler);
        _toggleInfo.UnregisterValueChangedCallback(filterHandler);
        _toggleAutoScroll.UnregisterValueChangedCallback(filterHandler);
    }

    private void SetDefaults()
    {
        _toggleInfo.value = true;
        _toggleMessage.value = true;
        _toggleDebug.value = true;
        _toggleWarning.value = true;
        _toggleError.value = true;
        _toggleAutoScroll.value = true;
    }
    
    private void FilterMessages()
    {
        // Start off with the original messages.
        foreach(var message in _consoleContent.Children())
        {
            if (message is LogEntry logEntry)
            {
                bool LogLevelPermitted = IsLogLevelEnabled(logEntry.logLevel);

                if (LogLevelPermitted)
                {
                    message.style.display = DisplayStyle.Flex;
                    if (!string.IsNullOrEmpty(searchFilter))
                    {
                        if (!logEntry.LogSource.SourceName.Contains(searchFilter) && !logEntry.LogMessage.Contains(searchFilter))
                        {
                            message.style.display = DisplayStyle.None;
                        }
                    }
                }
                else
                {
                    message.style.display = DisplayStyle.None;
                }
            }
        }
    }

    private bool IsLogLevelEnabled(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Error:
                return _toggleError.value;
            case LogLevel.Warning:
                return _toggleWarning.value;
            case LogLevel.Message:
                return _toggleMessage.value;
            case LogLevel.Info:
                return _toggleInfo.value;
            case LogLevel.Debug:
                return _toggleDebug.value;
            default:
                return true;
        }
    }

    internal Color GetColorFromLogLevel(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Fatal:
                return Color.red;
            case LogLevel.Error:
                return _spaceWarpPluginInstance.ConfigErrorColor.Value;
            case LogLevel.Warning:
                return _spaceWarpPluginInstance.ConfigWarningColor.Value;
            case LogLevel.Message:
                return _spaceWarpPluginInstance.ConfigMessageColor.Value;
            case LogLevel.Info:
                return _spaceWarpPluginInstance.ConfigInfoColor.Value;
            case LogLevel.Debug:
                return _spaceWarpPluginInstance.ConfigDebugColor.Value;
            default:
                return _spaceWarpPluginInstance.ConfigMessageColor.Value;
        }
    }

    private void ToggleAutoScroll()
    {
        _toggleAutoScroll.value = !_toggleAutoScroll.value;
        _logger.LogInfo("AutoScroll is now " + (_toggleAutoScroll.value ? "enabled" : "disabled") + ".");
    }

    private void AutoScrollToBottom()
    {
        StartCoroutine(AutoScrollToBottomCoroutine());
    }
    
    private IEnumerator AutoScrollToBottomCoroutine()
    {
        yield return null;

        _consoleContent.ScrollTo(_consoleContent.contentContainer[_consoleContent.contentContainer.childCount - 1]);
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