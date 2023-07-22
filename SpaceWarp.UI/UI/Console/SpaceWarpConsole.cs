using System.Collections;
using BepInEx.Logging;
using KSP.Game;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using static SpaceWarp.UI.Console.SpaceWarpConsoleLogListener;

namespace SpaceWarp.UI.Console;	

internal sealed class SpaceWarpConsole : KerbalMonoBehaviour	
{	
    // State
    private bool _isLoaded;
    private bool _isWindowVisible;

    // UITK Stuff
    private VisualElement _container;

    private TextField _consoleSearch;
    private string SearchFilter => _consoleSearch.text;
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
        foreach (var logMessage in LogMessages)
        {
            CreateNewLogEntry(logMessage);
        }
        // Binds the OnNewMessageReceived function to the OnNewMessage event
        OnNewLog += CreateNewLogEntry;
    }


    private void OnDestroy()
    {
        // Unbinds the OnNewMessageReceived function to the OnNewMessage event when destroyed
        OnNewLog -= CreateNewLogEntry;
    
    }

    private void CreateNewLogEntry(LogInfo logInfo)
    {
        LogEntry entry = new(logInfo)
        {
            TextColor = GetColorFromLogLevel(logInfo.Level)
        };
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
        // HUH?? See me after class
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
    
    private void FilterHandler (ChangeEvent<bool> evt) => FilterMessages();
    private void SearchHandler (ChangeEvent<string> evt) => FilterMessages();

    private void BindFunctions()
    {
        _consoleSearch.RegisterValueChangedCallback(SearchHandler);
        
        _toggleError.RegisterValueChangedCallback(FilterHandler);
        _toggleWarning.RegisterValueChangedCallback(FilterHandler);
        _toggleDebug.RegisterValueChangedCallback(FilterHandler);
        _toggleMessage.RegisterValueChangedCallback(FilterHandler);
        _toggleInfo.RegisterValueChangedCallback(FilterHandler);

        _toggleAutoScroll.RegisterValueChangedCallback(AutoScrollChanged);
    }

    private void UnbindFunctions()
    {
        //WHAT IS THIS?
        _consoleSearch.RegisterValueChangedCallback(SearchHandler);
        
        _toggleError.UnregisterValueChangedCallback(FilterHandler);
        _toggleWarning.UnregisterValueChangedCallback(FilterHandler);
        _toggleDebug.UnregisterValueChangedCallback(FilterHandler);
        _toggleMessage.UnregisterValueChangedCallback(FilterHandler);
        _toggleInfo.UnregisterValueChangedCallback(FilterHandler);

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
        for (var i = 0; i < _consoleContent.childCount; i++)
        {
            //Only LogEntries should be in this content

            if (_consoleContent[i] is LogEntry entry)
            {
                CheckFilter(entry);
            }
        }
    }

    private void CheckFilter(LogEntry logEntry)
    {
        var logLevelPermitted = IsLogLevelEnabled(logEntry.LogLevel);

        if (logLevelPermitted)
        {
            logEntry.style.display = DisplayStyle.Flex;
            if (string.IsNullOrEmpty(SearchFilter)) return;
            var lowercaseSearch = SearchFilter.ToLower();
            if (logEntry.LogSource.SourceName.ToLower().Contains(lowercaseSearch) ||
                logEntry.LogMessage.ToLower().Contains(lowercaseSearch)) return;
            logEntry.style.display = DisplayStyle.None;
        }
        else
        {
            logEntry.style.display = DisplayStyle.None;
        }
    }

    private bool IsLogLevelEnabled(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Error => _toggleError.value,
            LogLevel.Warning => _toggleWarning.value,
            LogLevel.Message => _toggleMessage.value,
            LogLevel.Info => _toggleInfo.value,
            LogLevel.Debug => _toggleDebug.value,
            _ => true
        };
    }

    private static Color GetColorFromLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Fatal => Color.red,
            LogLevel.Error => SpaceWarpPlugin.Instance.ConfigErrorColor.Value,
            LogLevel.Warning => SpaceWarpPlugin.Instance.ConfigWarningColor.Value,
            LogLevel.Message => SpaceWarpPlugin.Instance.ConfigMessageColor.Value,
            LogLevel.Info => SpaceWarpPlugin.Instance.ConfigInfoColor.Value,
            LogLevel.Debug => SpaceWarpPlugin.Instance.ConfigDebugColor.Value,
            _ => SpaceWarpPlugin.Instance.ConfigMessageColor.Value
        };
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