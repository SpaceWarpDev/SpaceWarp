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

    private bool _autoScroll = true;
    private string _searchText = string.Empty;
    private bool _showDebug = true;
    private bool _showError = true;
    private bool _showInfo = true;
    private bool _showMessage = true;
    private bool _showWarning = true;
    private List<SpaceWarpConsoleEntryViewModel> _visibleEntries = new();

    public event Action? CloseRequested;
    public event Action? ClearRequested;
    public event Action? EntriesChanged;

    public SpaceWarpConsoleViewModel()
    {
        CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());
        ClearCommand = new RelayCommand(() => ClearRequested?.Invoke());
    }

    [CreateProperty] public RelayCommand CloseCommand { get; }
    [CreateProperty] public RelayCommand ClearCommand { get; }

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

    [CreateProperty] public string SummaryText => $"{VisibleEntries.Count} visible / {_allEntries.Count} total";

    public void InitializeFromLogs(IEnumerable<SpaceWarpConsoleLogListener.LogInfo> logs)
    {
        _allEntries.Clear();
        _allEntries.AddRange(logs.Select(SpaceWarpConsoleEntryViewModel.FromLogInfo));
        RebuildVisibleEntries();
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

    private void TrimToLimit()
    {
        int limit = UI.Instance.ConfigDebugMessageLimit.Value;
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
