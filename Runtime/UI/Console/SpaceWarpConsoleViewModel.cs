using System;
using System.Collections.Generic;
using System.Linq;
using ReduxLib.Logging;
using UitkForKsp2.MVVM.Commands;
using UitkForKsp2.MVVM.Core;
using Unity.Properties;

namespace SpaceWarp2.UI.Console;

internal sealed class SpaceWarpConsoleViewModel : ViewModelBase
{
    private const int MaxLuaOutputLength = 12000;

    private readonly List<SpaceWarpConsoleEntryViewModel> _allEntries = new();

    private SpaceWarpConsoleTab _activeTab = SpaceWarpConsoleTab.Logs;
    private bool _autoScroll = true;
    private bool _isCompact;
    private string _csharpCode = "Game.GlobalGameState.GetState()";
    private string _csharpOutput = string.Empty;
    private string _luaCode = "local info = View.ActiveVehicle.GetInfo()\nlocal telemetry = View.ActiveVehicle.GetTelemetry()\nScript.Log.Info(\"Controlling \" .. info.displayName)\nreturn telemetry.altitudeSeaLevel";
    private string _luaOutput = string.Empty;
    private string _luaSelectedScriptPath = string.Empty;
    private bool _isLuaRunning;
    private string _searchText = string.Empty;
    private bool _showDebug = true;
    private bool _showError = true;
    private bool _showInfo = true;
    private bool _showMessage = true;
    private bool _showWarning = true;
    private string _activitySignature = string.Empty;
    private List<SpaceWarpConsoleCliActivityEntryViewModel> _activityEntries = new();
    private List<string> _luaScriptFiles = new();
    private List<SpaceWarpConsoleEntryViewModel> _visibleEntries = new();

    public event Action? CloseRequested;
    public event Action? ClearRequested;
    public event Action? EntriesChanged;
    public event Action? ActivityEntriesChanged;
    public event Action? LuaScriptsChanged;
    public event Action? LuaRunStateChanged;
    public event Action? DisplayStateChanged;
    public event Action? CSharpOutputChanged;
    public event Action? LuaOutputChanged;

    public SpaceWarpConsoleViewModel()
    {
        CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());
        ClearCommand = new RelayCommand(() => ClearRequested?.Invoke());
        ToggleCompactCommand = new RelayCommand(ToggleCompact);
        ShowLogsCommand = new RelayCommand(ShowLogs);
        ShowCSharpCommand = new RelayCommand(ShowCSharp);
        ShowLuaCommand = new RelayCommand(ShowLua);
        RunCSharpCommand = new RelayCommand(RunCSharp);
        ResetCSharpCommand = new RelayCommand(ResetCSharp);
        RunLuaCommand = new RelayCommand(RunLua);
        StopLuaCommand = new RelayCommand(StopLua);
        ResetLuaCommand = new RelayCommand(ResetLua);
        RefreshLuaScriptsCommand = new RelayCommand(RefreshLuaScriptsFromCommand);
        LoadLuaScriptCommand = new RelayCommand(LoadLuaScript);
        SaveLuaScriptCommand = new RelayCommand(SaveLuaScript);
        NewLuaScriptCommand = new RelayCommand(NewLuaScript);
        OpenLuaScriptFolderCommand = new RelayCommand(OpenLuaScriptFolder);
        SpaceWarpConsoleLuaService.LuaOutputReceived += AppendLuaOutput;
    }

    [CreateProperty] public RelayCommand CloseCommand { get; }
    [CreateProperty] public RelayCommand ClearCommand { get; }
    [CreateProperty] public RelayCommand ToggleCompactCommand { get; }
    [CreateProperty] public RelayCommand ShowLogsCommand { get; }
    [CreateProperty] public RelayCommand ShowCSharpCommand { get; }
    [CreateProperty] public RelayCommand ShowLuaCommand { get; }
    [CreateProperty] public RelayCommand RunCSharpCommand { get; }
    [CreateProperty] public RelayCommand ResetCSharpCommand { get; }
    [CreateProperty] public RelayCommand RunLuaCommand { get; }
    [CreateProperty] public RelayCommand StopLuaCommand { get; }
    [CreateProperty] public RelayCommand ResetLuaCommand { get; }
    [CreateProperty] public RelayCommand RefreshLuaScriptsCommand { get; }
    [CreateProperty] public RelayCommand LoadLuaScriptCommand { get; }
    [CreateProperty] public RelayCommand SaveLuaScriptCommand { get; }
    [CreateProperty] public RelayCommand NewLuaScriptCommand { get; }
    [CreateProperty] public RelayCommand OpenLuaScriptFolderCommand { get; }

    [CreateProperty]
    public bool ShowDebug
    {
        get => _showDebug;
        set
        {
            if (!SetField(ref _showDebug, value))
            {
                return;
            }

            RebuildVisibleEntries();
        }
    }

    [CreateProperty]
    public bool ShowInfo
    {
        get => _showInfo;
        set
        {
            if (!SetField(ref _showInfo, value))
            {
                return;
            }

            RebuildVisibleEntries();
        }
    }

    [CreateProperty]
    public bool ShowMessage
    {
        get => _showMessage;
        set
        {
            if (!SetField(ref _showMessage, value))
            {
                return;
            }

            RebuildVisibleEntries();
        }
    }

    [CreateProperty]
    public bool ShowWarning
    {
        get => _showWarning;
        set
        {
            if (!SetField(ref _showWarning, value))
            {
                return;
            }

            RebuildVisibleEntries();
        }
    }

    [CreateProperty]
    public bool ShowError
    {
        get => _showError;
        set
        {
            if (!SetField(ref _showError, value))
            {
                return;
            }

            RebuildVisibleEntries();
        }
    }

    [CreateProperty]
    public bool AutoScroll
    {
        get => _autoScroll;
        set => SetField(ref _autoScroll, value);
    }

    [CreateProperty]
    public string SearchText
    {
        get => _searchText;
        set
        {
            value ??= string.Empty;
            if (!SetField(ref _searchText, value))
            {
                return;
            }

            RebuildVisibleEntries();
        }
    }

    [CreateProperty]
    public List<SpaceWarpConsoleEntryViewModel> VisibleEntries
    {
        get => _visibleEntries;
        private set => SetField(ref _visibleEntries, value);
    }

    [CreateProperty]
    public string CSharpCode
    {
        get => _csharpCode;
        set => SetField(ref _csharpCode, value ?? string.Empty);
    }

    [CreateProperty]
    public string CSharpOutput
    {
        get => _csharpOutput;
        private set
        {
            if (SetField(ref _csharpOutput, value ?? string.Empty))
            {
                CSharpOutputChanged?.Invoke();
            }
        }
    }

    [CreateProperty]
    public string LuaCode
    {
        get => _luaCode;
        set => SetField(ref _luaCode, value ?? string.Empty);
    }

    [CreateProperty]
    public string LuaOutput
    {
        get => _luaOutput;
        private set
        {
            if (SetField(ref _luaOutput, value ?? string.Empty))
            {
                LuaOutputChanged?.Invoke();
            }
        }
    }

    [CreateProperty]
    public string LuaSelectedScriptPath
    {
        get => _luaSelectedScriptPath;
        set => SetField(ref _luaSelectedScriptPath, NormalizeLuaPath(value));
    }

    [CreateProperty]
    public List<SpaceWarpConsoleCliActivityEntryViewModel> ActivityEntries
    {
        get => _activityEntries;
        private set => SetField(ref _activityEntries, value);
    }

    [CreateProperty]
    public List<string> LuaScriptFiles
    {
        get => _luaScriptFiles;
        private set => SetField(ref _luaScriptFiles, value);
    }

    [CreateProperty] public bool IsCompact => _isCompact;
    [CreateProperty] public bool IsLuaRunning => _isLuaRunning;
    [CreateProperty] public bool IsShowingLogs => _activeTab == SpaceWarpConsoleTab.Logs;
    [CreateProperty] public bool IsShowingCSharp => _activeTab == SpaceWarpConsoleTab.CSharp;
    [CreateProperty] public bool IsShowingLua => _activeTab == SpaceWarpConsoleTab.Lua;
    [CreateProperty] public string CompactToggleText => _isCompact ? "Restore" : "Compact";
    [CreateProperty] public string SummaryText => $"{VisibleEntries.Count} visible / {_allEntries.Count} total";
    [CreateProperty] public string ActivitySummaryText => $"{ActivityEntries.Count} entries";
    [CreateProperty] public string LuaFileSummaryText => $"{LuaScriptFiles.Count} files";

    /// <summary>
    /// Detaches this view model from static events. Must be called when the owning console is
    /// destroyed, otherwise the handler leaks onto the static event (UDR0004), which duplicates
    /// when the console is recreated with Domain Reload disabled.
    /// </summary>
    public void Cleanup()
    {
        SpaceWarpConsoleLuaService.LuaOutputReceived -= AppendLuaOutput;
    }

    public void InitializeFromLogs(IEnumerable<SpaceWarpConsoleLogListener.LogInfo> logs)
    {
        _allEntries.Clear();
        _allEntries.AddRange(logs.Select(SpaceWarpConsoleEntryViewModel.FromLogInfo));
        RebuildVisibleEntries();
        PollCliIntegrationActivity();
        RefreshLuaScripts();
    }

    public void AddLog(SpaceWarpConsoleLogListener.LogInfo info)
    {
        _allEntries.Add(SpaceWarpConsoleEntryViewModel.FromLogInfo(info));
        TrimToLimit();
        RebuildVisibleEntries();
    }

    public void ClearLogs()
    {
        _allEntries.Clear();
        RebuildVisibleEntries();
    }

    public void PollCliIntegrationActivity()
    {
        IReadOnlyList<SpaceWarpConsoleCliActivityEntryViewModel> entries =
            SpaceWarpConsoleCliIntegrationBridge.GetActivityEntries();
        string signature = string.Join("|", entries.Select(entry => entry.Signature));
        if (signature == _activitySignature)
        {
            return;
        }

        _activitySignature = signature;
        ActivityEntries = entries.ToList();
        Notify(nameof(ActivitySummaryText));
        ActivityEntriesChanged?.Invoke();
    }

    private void ShowLogs()
    {
        SetActiveTab(SpaceWarpConsoleTab.Logs);
    }

    private void ShowCSharp()
    {
        SetActiveTab(SpaceWarpConsoleTab.CSharp);
        PollCliIntegrationActivity();
    }

    private void ShowLua()
    {
        SetActiveTab(SpaceWarpConsoleTab.Lua);
        RefreshLuaScripts();
    }

    public void TickLuaExecution()
    {
        SpaceWarpConsoleCSharpResult result = SpaceWarpConsoleLuaService.TickLua();
        if (!string.IsNullOrWhiteSpace(result.DisplayText))
        {
            AppendLuaOutput(result.DisplayText);
        }

        SyncLuaRunningState();
    }

    private void ToggleCompact()
    {
        _isCompact = !_isCompact;
        NotifyDisplayStateChanged();
    }

    private void RunCSharp()
    {
        object? activity = SpaceWarpConsoleCliIntegrationBridge.AddActivity("UI", "Eval", "eval.csharp", CSharpCode);
        try
        {
            SpaceWarpConsoleCSharpResult result = SpaceWarpConsoleCliIntegrationBridge.EvaluateCSharp(CSharpCode, 4);
            CSharpOutput = result.DisplayText;
            SpaceWarpConsoleCliIntegrationBridge.CompleteActivity(activity, result.Success, CSharpOutput);
        }
        catch (Exception ex)
        {
            CSharpOutput = ex.GetBaseException().Message;
            SpaceWarpConsoleCliIntegrationBridge.CompleteActivity(activity, false, CSharpOutput);
        }

        PollCliIntegrationActivity();
    }

    private void ResetCSharp()
    {
        SpaceWarpConsoleCliIntegrationBridge.ResetCSharp();
        CSharpOutput = "C# session reset.";
    }

    private void RunLua()
    {
        object? activity = SpaceWarpConsoleCliIntegrationBridge.AddActivity("UI", "Lua", "eval.lua", LuaCode);
        LuaOutput = "Started.";
        try
        {
            SpaceWarpConsoleCSharpResult result = SpaceWarpConsoleLuaService.RunLua(LuaCode);
            if (!result.Success)
            {
                LuaOutput = result.DisplayText;
            }
            else if (!IsLuaStartResult(result.DisplayText))
            {
                AppendLuaOutput(result.DisplayText);
            }

            SpaceWarpConsoleCliIntegrationBridge.CompleteActivity(activity, result.Success, LuaOutput);
        }
        catch (Exception ex)
        {
            LuaOutput = ex.GetBaseException().Message;
            SpaceWarpConsoleCliIntegrationBridge.CompleteActivity(activity, false, LuaOutput);
        }

        SyncLuaRunningState();
        PollCliIntegrationActivity();
    }

    private void StopLua()
    {
        SpaceWarpConsoleCSharpResult result = SpaceWarpConsoleLuaService.StopLua();
        AppendLuaOutput(result.DisplayText);
        SyncLuaRunningState();
    }

    private void ResetLua()
    {
        LuaOutput = string.Empty;
        RefreshLuaScripts();
    }

    public void SelectLuaScript(string path)
    {
        LuaSelectedScriptPath = path;
    }

    private void RefreshLuaScripts()
    {
        LuaScriptFiles = SpaceWarpConsoleLuaService.ListLuaScripts().ToList();
        Notify(nameof(LuaFileSummaryText));
        LuaScriptsChanged?.Invoke();
    }

    private void RefreshLuaScriptsFromCommand()
    {
        RefreshLuaScripts();
        AppendLuaOutput(LuaScriptFiles.Count == 0
            ? "No scripts found."
            : $"Refreshed {LuaScriptFiles.Count} Lua script{(LuaScriptFiles.Count == 1 ? string.Empty : "s")}.");
    }

    private void LoadLuaScript()
    {
        SpaceWarpConsoleCSharpResult result = SpaceWarpConsoleLuaService.ReadLuaScript(LuaSelectedScriptPath);
        if (result.Success)
        {
            LuaCode = result.DisplayText;
            AppendLuaOutput("Loaded " + LuaSelectedScriptPath + ".");
        }
        else
        {
            AppendLuaOutput(result.DisplayText);
        }
    }

    private void SaveLuaScript()
    {
        SpaceWarpConsoleCSharpResult result = SpaceWarpConsoleLuaService.WriteLuaScript(LuaSelectedScriptPath, LuaCode);
        AppendLuaOutput(result.DisplayText);
        RefreshLuaScripts();
    }

    private void NewLuaScript()
    {
        SpaceWarpConsoleCSharpResult result = SpaceWarpConsoleLuaService.CreateLuaScript();
        if (result.Success)
        {
            LuaSelectedScriptPath = result.DisplayText;
            LuaCode = "-- New Lua script\n";
            AppendLuaOutput("Created " + LuaSelectedScriptPath + ".");
        }
        else
        {
            AppendLuaOutput(result.DisplayText);
        }

        RefreshLuaScripts();
    }

    private void OpenLuaScriptFolder()
    {
        SpaceWarpConsoleCSharpResult result = SpaceWarpConsoleLuaService.OpenLuaScriptFolder();
        AppendLuaOutput(result.Success ? "Opened " + result.DisplayText + "." : result.DisplayText);
    }

    private void SetActiveTab(SpaceWarpConsoleTab tab)
    {
        bool changed = _activeTab != tab || _isCompact;
        if (!changed)
        {
            return;
        }

        _activeTab = tab;
        _isCompact = false;
        NotifyDisplayStateChanged();
    }

    private void NotifyDisplayStateChanged()
    {
        Notify(nameof(IsCompact));
        Notify(nameof(IsShowingLogs));
        Notify(nameof(IsShowingCSharp));
        Notify(nameof(IsShowingLua));
        Notify(nameof(CompactToggleText));
        DisplayStateChanged?.Invoke();
    }

    private void TrimToLimit()
    {
        int limit = UI.Instance.ConfigDebugMessageLimit;
        if (_allEntries.Count <= limit)
        {
            return;
        }

        _allEntries.RemoveRange(0, _allEntries.Count - limit);
    }

    private void RebuildVisibleEntries()
    {
        IEnumerable<SpaceWarpConsoleEntryViewModel> entries = _allEntries.Where(MatchesFilters);
        string searchText = SearchText.Trim();
        if (!string.IsNullOrEmpty(searchText))
        {
            entries = entries.Where(entry => entry.MatchesSearch(searchText));
        }

        VisibleEntries = entries.ToList();
        Notify(nameof(SummaryText));
        EntriesChanged?.Invoke();
    }

    private bool MatchesFilters(SpaceWarpConsoleEntryViewModel entry)
    {
        return entry.Level switch
        {
            LogLevel.Debug => ShowDebug,
            LogLevel.Info => ShowInfo,
            LogLevel.Message => ShowMessage,
            LogLevel.Warning => ShowWarning,
            LogLevel.Error => ShowError,
            LogLevel.Fatal => ShowError,
            _ => true
        };
    }

    private static string NormalizeLuaPath(string value)
    {
        return (value ?? string.Empty).Trim().Replace('\\', '/');
    }

    private void AppendLuaOutput(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        string output = string.IsNullOrWhiteSpace(LuaOutput)
            ? line
            : LuaOutput + Environment.NewLine + line;
        if (output.Length > MaxLuaOutputLength)
        {
            output = output[^MaxLuaOutputLength..];
        }

        LuaOutput = output;
    }

    private void SyncLuaRunningState()
    {
        bool isRunning = SpaceWarpConsoleLuaService.IsLuaRunning;
        if (!SetField(ref _isLuaRunning, isRunning, nameof(IsLuaRunning)))
        {
            return;
        }

        LuaRunStateChanged?.Invoke();
    }

    private static bool IsLuaStartResult(string text)
    {
        return text.StartsWith("Started.", StringComparison.OrdinalIgnoreCase);
    }
}

internal enum SpaceWarpConsoleTab
{
    Logs,
    CSharp,
    Lua
}
