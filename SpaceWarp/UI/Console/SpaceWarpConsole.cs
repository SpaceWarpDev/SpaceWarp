using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Bootstrap;	
using BepInEx.Logging;
using KSP.Animation;
using KSP.DebugTools;
using KSP.Game;
using MoonSharp.VsCodeDebugger.SDK;
using SpaceWarp.API.Assets;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using static SpaceWarp.UI.Console.SpaceWarpConsoleLogListener;

namespace SpaceWarp.UI.Console;	

public sealed class SpaceWarpConsole : KerbalMonoBehaviour	
{	
    // State
    private bool _isLoaded;
    private bool _isWindowVisible;

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
        _consoleContent.Add(entry);

        //Check if this entry should be currently hidden
        CheckFilter(entry);

        //First in first out
        if (_consoleContent.contentContainer.childCount > SpaceWarpPlugin.Instance.ConfigDebugMessageLimit.Value)
            _consoleContent.contentContainer.RemoveAt(0); 
        if(_toggleAutoScroll.value)
            AutoScrollToBottom();
    }


    private void Awake()
    {
        if (_isLoaded) return;
        
        // Debugging
        _logger = SpaceWarpPlugin.Logger;

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

        _toggleAutoScroll.RegisterValueChangedCallback(AutoScrollChanged);
    }

    private void UnbindFunctions()
    {
        _consoleSearch.RegisterValueChangedCallback(searchHandler);
        
        _toggleError.UnregisterValueChangedCallback(filterHandler);
        _toggleWarning.UnregisterValueChangedCallback(filterHandler);
        _toggleDebug.UnregisterValueChangedCallback(filterHandler);
        _toggleMessage.UnregisterValueChangedCallback(filterHandler);
        _toggleInfo.UnregisterValueChangedCallback(filterHandler);

        _toggleAutoScroll.UnregisterValueChangedCallback(AutoScrollChanged);
    }

    private void AutoScrollChanged(ChangeEvent<bool> changeEvent)
    {
        if (changeEvent.newValue)
        {
            StartCoroutine(AutoScrollToBottomCoroutine());
        }
        else
        {
            StopCoroutine(AutoScrollToBottomCoroutine());
        }
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
        for (int i = 0; i < _consoleContent.childCount; i++)
        {
            //Only LogEntries should be in this content
            LogEntry entry = _consoleContent[i] as LogEntry;

            if (entry is not null)
            {
                CheckFilter(entry);
            }
        }
    }
    internal void CheckFilter(LogEntry logEntry)
    {
        bool LogLevelPermitted = IsLogLevelEnabled(logEntry.logLevel);

        if (LogLevelPermitted)
        {
            logEntry.style.display = DisplayStyle.Flex;
            if (!string.IsNullOrEmpty(searchFilter))
            {
                string lowercaseSearch = searchFilter.ToLower();
                if (!logEntry.LogSource.SourceName.ToLower().Contains(lowercaseSearch) && !logEntry.LogMessage.ToLower().Contains(lowercaseSearch))
                {
                    logEntry.style.display = DisplayStyle.None;
                }
            }
        }
        else
        {
            logEntry.style.display = DisplayStyle.None;
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
                return SpaceWarpPlugin.Instance.ConfigErrorColor.Value;
            case LogLevel.Warning:
                return SpaceWarpPlugin.Instance.ConfigWarningColor.Value;
            case LogLevel.Message:
                return SpaceWarpPlugin.Instance.ConfigMessageColor.Value;
            case LogLevel.Info:
                return SpaceWarpPlugin.Instance.ConfigInfoColor.Value;
            case LogLevel.Debug:
                return SpaceWarpPlugin.Instance.ConfigDebugColor.Value;
            default:
                return SpaceWarpPlugin.Instance.ConfigMessageColor.Value;
        }
    }

    private void AutoScrollToBottom()
    {
        StartCoroutine(AutoScrollToBottomCoroutine());
    }
    
    private IEnumerator AutoScrollToBottomCoroutine()
    {
        yield return null;
        _consoleContent.ScrollTo(_consoleContent.contentContainer[_consoleContent.contentContainer.childCount - 1]);
        //put a while loop here to make this automatic!
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