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
    private readonly List<SpaceWarpConsoleEntryViewModel> _allEntries = new();

    private SpaceWarpConsoleTab _activeTab = SpaceWarpConsoleTab.Logs;
    private bool _autoScroll = true;
    private string _csharpCode = "Game.GlobalGameState.GetState()";
    private string _csharpOutput = string.Empty;
    private string _searchText = string.Empty;
    private bool _showDebug = true;
    private bool _showError = true;
    private bool _showInfo = true;
    private bool _showMessage = true;
    private bool _showWarning = true;
    private string _activitySignature = string.Empty;
    private List<SpaceWarpConsoleCliActivityEntryViewModel> _activityEntries = new();
    private List<SpaceWarpConsoleEntryViewModel> _visibleEntries = new();

    public event Action? CloseRequested;
    public event Action? ClearRequested;
    public event Action? EntriesChanged;
    public event Action? ActivityEntriesChanged;
    public event Action? DisplayStateChanged;

    public SpaceWarpConsoleViewModel()
    {
        CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());
        ClearCommand = new RelayCommand(() => ClearRequested?.Invoke());
        ShowLogsCommand = new RelayCommand(ShowLogs);
        ShowCSharpCommand = new RelayCommand(ShowCSharp);
        RunCSharpCommand = new RelayCommand(RunCSharp);
        ResetCSharpCommand = new RelayCommand(ResetCSharp);
    }

    [CreateProperty] public RelayCommand CloseCommand { get; }
    [CreateProperty] public RelayCommand ClearCommand { get; }
    [CreateProperty] public RelayCommand ShowLogsCommand { get; }
    [CreateProperty] public RelayCommand ShowCSharpCommand { get; }
    [CreateProperty] public RelayCommand RunCSharpCommand { get; }
    [CreateProperty] public RelayCommand ResetCSharpCommand { get; }

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
        private set => SetField(ref _csharpOutput, value ?? string.Empty);
    }

    [CreateProperty]
    public List<SpaceWarpConsoleCliActivityEntryViewModel> ActivityEntries
    {
        get => _activityEntries;
        private set => SetField(ref _activityEntries, value);
    }

    [CreateProperty] public bool IsShowingLogs => _activeTab == SpaceWarpConsoleTab.Logs;
    [CreateProperty] public bool IsShowingCSharp => _activeTab == SpaceWarpConsoleTab.CSharp;
    [CreateProperty] public string SummaryText => $"{VisibleEntries.Count} visible / {_allEntries.Count} total";
    [CreateProperty] public string ActivitySummaryText => $"{ActivityEntries.Count} entries";

    public void InitializeFromLogs(IEnumerable<SpaceWarpConsoleLogListener.LogInfo> logs)
    {
        _allEntries.Clear();
        _allEntries.AddRange(logs.Select(SpaceWarpConsoleEntryViewModel.FromLogInfo));
        RebuildVisibleEntries();
        PollCliIntegrationActivity();
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

    private void SetActiveTab(SpaceWarpConsoleTab tab)
    {
        if (_activeTab == tab)
        {
            return;
        }

        _activeTab = tab;
        Notify(nameof(IsShowingLogs));
        Notify(nameof(IsShowingCSharp));
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
}

internal enum SpaceWarpConsoleTab
{
    Logs,
    CSharp
}
