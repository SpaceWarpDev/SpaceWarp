using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using LogLevel = ReduxLib.Logging.LogLevel;

namespace SpaceWarp2.UI.Console;

internal sealed class SpaceWarpConsoleView : System.IDisposable
{
    private static readonly string[] RowStyleClasses =
    {
        "console-row-debug",
        "console-row-info",
        "console-row-message-level",
        "console-row-warning",
        "console-row-error",
        "console-row-fatal"
    };

    private readonly UIDocument _window;
    private readonly SpaceWarpConsoleViewModel _viewModel;

    private VisualElement? _root;
    private Button? _logsTab;
    private Button? _csharpTab;
    private Button? _luaTab;
    private VisualElement? _logsPanel;
    private VisualElement? _csharpPanel;
    private VisualElement? _luaPanel;
    private ListView? _logList;
    private ListView? _activityList;
    private ListView? _luaScriptList;
    private ScrollView? _csharpOutputScrollView;
    private ScrollView? _luaOutputScrollView;
    private Button? _luaRunButton;
    private Button? _luaStopButton;
    private Button? _compactLuaRunButton;
    private Button? _compactLuaStopButton;
    private Label? _emptyState;
    private Label? _activityEmptyState;
    private Label? _luaScriptEmptyState;
    private VisualElement? _debugChip;
    private VisualElement? _infoChip;
    private VisualElement? _messageChip;
    private VisualElement? _warningChip;
    private VisualElement? _errorChip;
    private bool _isOpen;

    public SpaceWarpConsoleView(UIDocument window, SpaceWarpConsoleViewModel viewModel)
    {
        _window = window;
        _viewModel = viewModel;
        _viewModel.EntriesChanged += RefreshEntries;
        _viewModel.ActivityEntriesChanged += RefreshActivityEntries;
        _viewModel.LuaScriptsChanged += RefreshLuaScripts;
        _viewModel.LuaRunStateChanged += RefreshLuaRunState;
        _viewModel.DisplayStateChanged += RefreshDisplayState;
        _viewModel.CSharpOutputChanged += ScrollCSharpOutputToBottomIfPinned;
        _viewModel.LuaOutputChanged += ScrollLuaOutputToBottomIfPinned;
    }

    public bool IsOpen => _isOpen;

    public void Load()
    {
        _window.rootVisualElement.dataSource = _viewModel;
        _window.EnableLocalization();
        CacheVisualElements();
        ConfigureListView();
        RefreshEntries();
        RefreshActivityEntries();
        RefreshLuaScripts();
        RefreshLuaRunState();
        RefreshDisplayState();
        Hide();
        _root?.CenterByDefault();
    }

    public void Dispose()
    {
        _viewModel.EntriesChanged -= RefreshEntries;
        _viewModel.ActivityEntriesChanged -= RefreshActivityEntries;
        _viewModel.LuaScriptsChanged -= RefreshLuaScripts;
        _viewModel.LuaRunStateChanged -= RefreshLuaRunState;
        _viewModel.DisplayStateChanged -= RefreshDisplayState;
        _viewModel.CSharpOutputChanged -= ScrollCSharpOutputToBottomIfPinned;
        _viewModel.LuaOutputChanged -= ScrollLuaOutputToBottomIfPinned;
    }

    public void Show()
    {
        _window.Show();
        _root?.Show();
        _isOpen = true;
    }

    public void Hide()
    {
        _root?.Hide();
        _window.Hide();
        _isOpen = false;
    }

    public void ScrollToBottom()
    {
        if (_logList == null || _viewModel.VisibleEntries.Count == 0)
        {
            return;
        }

        _logList.schedule.Execute(() => _logList.ScrollToItem(_viewModel.VisibleEntries.Count - 1));
    }

    private void CacheVisualElements()
    {
        _root = _window.rootVisualElement.Q<VisualElement>("root");
        _logsTab = _root?.Q<Button>("logs-tab");
        _csharpTab = _root?.Q<Button>("csharp-tab");
        _luaTab = _root?.Q<Button>("lua-tab");
        _logsPanel = _root?.Q<VisualElement>("logs-panel");
        _csharpPanel = _root?.Q<VisualElement>("csharp-panel");
        _luaPanel = _root?.Q<VisualElement>("lua-panel");
        _logList = _root?.Q<ListView>("console-list");
        _activityList = _root?.Q<ListView>("cli-activity-list");
        _luaScriptList = _root?.Q<ListView>("lua-script-list");
        _csharpOutputScrollView = _root?.Q<ScrollView>("csharp-output-scroll");
        _luaOutputScrollView = _root?.Q<ScrollView>("lua-output-scroll");
        _luaRunButton = _root?.Q<Button>("lua-run");
        _luaStopButton = _root?.Q<Button>("lua-stop");
        _compactLuaRunButton = _root?.Q<Button>("compact-lua-run");
        _compactLuaStopButton = _root?.Q<Button>("compact-lua-stop");
        _emptyState = _root?.Q<Label>("empty-state");
        _activityEmptyState = _root?.Q<Label>("cli-activity-empty-state");
        _luaScriptEmptyState = _root?.Q<Label>("lua-script-empty-state");
        _debugChip = _root?.Q<VisualElement>("toggle-debug");
        _infoChip = _root?.Q<VisualElement>("toggle-info");
        _messageChip = _root?.Q<VisualElement>("toggle-message");
        _warningChip = _root?.Q<VisualElement>("toggle-warning");
        _errorChip = _root?.Q<VisualElement>("toggle-error");
        _csharpOutputScrollView?.EnablePinnedBottomAutoScroll();
        _luaOutputScrollView?.EnablePinnedBottomAutoScroll();
        BindFilterChips();
    }

    private void BindFilterChips()
    {
        BindFilterChip(_debugChip, () => _viewModel.ShowDebug = !_viewModel.ShowDebug);
        BindFilterChip(_infoChip, () => _viewModel.ShowInfo = !_viewModel.ShowInfo);
        BindFilterChip(_messageChip, () => _viewModel.ShowMessage = !_viewModel.ShowMessage);
        BindFilterChip(_warningChip, () => _viewModel.ShowWarning = !_viewModel.ShowWarning);
        BindFilterChip(_errorChip, () => _viewModel.ShowError = !_viewModel.ShowError);
    }

    private static void BindFilterChip(VisualElement? chip, System.Action toggleAction)
    {
        if (chip == null)
        {
            return;
        }

        chip.AddManipulator(new Clickable(toggleAction));
    }

    private void ConfigureListView()
    {
        if (_logList == null)
        {
            return;
        }

        _logList.bindItem = (element, index) => ApplyRow(
            element,
            _viewModel.VisibleEntries[index]
        );

        if (_activityList != null)
        {
            _activityList.makeItem = CreateActivityRow;
            _activityList.bindItem = (element, index) => ApplyActivityRow(
                element,
                _viewModel.ActivityEntries[index]
            );
        }

        if (_luaScriptList != null)
        {
            _luaScriptList.makeItem = CreateLuaScriptRow;
            _luaScriptList.bindItem = (element, index) => ApplyLuaScriptRow(
                element,
                _viewModel.LuaScriptFiles[index]
            );
            _luaScriptList.selectionChanged += selection =>
            {
                foreach (object selected in selection)
                {
                    _viewModel.SelectLuaScript(selected?.ToString() ?? string.Empty);
                    break;
                }
            };
        }
    }

    private void RefreshEntries()
    {
        if (_logList == null)
        {
            return;
        }

        _logList.itemsSource = _viewModel.VisibleEntries;
        _logList.RefreshItems();
        if (_emptyState != null)
        {
            _emptyState.style.display = _viewModel.VisibleEntries.Count == 0
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        SyncFilterButtons();
    }

    private void RefreshActivityEntries()
    {
        if (_activityList == null)
        {
            return;
        }

        _activityList.itemsSource = _viewModel.ActivityEntries;
        _activityList.RefreshItems();
        if (_activityEmptyState != null)
        {
            _activityEmptyState.style.display = _viewModel.ActivityEntries.Count == 0
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }

    private void RefreshLuaScripts()
    {
        if (_luaScriptList == null)
        {
            return;
        }

        _luaScriptList.itemsSource = _viewModel.LuaScriptFiles;
        _luaScriptList.RefreshItems();
        if (_luaScriptEmptyState != null)
        {
            _luaScriptEmptyState.style.display = _viewModel.LuaScriptFiles.Count == 0
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }

    private void RefreshLuaRunState()
    {
        _luaRunButton?.SetEnabled(!_viewModel.IsLuaRunning);
        _luaStopButton?.SetEnabled(_viewModel.IsLuaRunning);
        _compactLuaRunButton?.SetEnabled(!_viewModel.IsLuaRunning);
        _compactLuaStopButton?.SetEnabled(_viewModel.IsLuaRunning);
    }

    private void RefreshDisplayState()
    {
        if (_root != null)
        {
            if (_viewModel.IsCompact)
            {
                _root.AddToClassList("console-compact");
            }
            else
            {
                _root.RemoveFromClassList("console-compact");
            }

            _root.EnableInClassList("console-lua-active", _viewModel.IsShowingLua);
        }

        SetTabState(_logsTab, _viewModel.IsShowingLogs);
        SetTabState(_csharpTab, _viewModel.IsShowingCSharp);
        SetTabState(_luaTab, _viewModel.IsShowingLua);

        if (_logsPanel != null)
        {
            _logsPanel.style.display = _viewModel.IsShowingLogs ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (_csharpPanel != null)
        {
            _csharpPanel.style.display = _viewModel.IsShowingCSharp ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (_luaPanel != null)
        {
            _luaPanel.style.display = _viewModel.IsShowingLua ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (_viewModel.IsShowingCSharp)
        {
            ScrollCSharpOutputToBottomIfPinned();
        }

        if (_viewModel.IsShowingLua)
        {
            ScrollLuaOutputToBottomIfPinned();
        }
    }

    private void ScrollCSharpOutputToBottomIfPinned()
    {
        _csharpOutputScrollView.ScrollToBottomIfPinned();
    }

    private void ScrollLuaOutputToBottomIfPinned()
    {
        _luaOutputScrollView.ScrollToBottomIfPinned();
    }

    private void SyncFilterButtons()
    {
        SetFilterChipState(_debugChip, _viewModel.ShowDebug);
        SetFilterChipState(_infoChip, _viewModel.ShowInfo);
        SetFilterChipState(_messageChip, _viewModel.ShowMessage);
        SetFilterChipState(_warningChip, _viewModel.ShowWarning);
        SetFilterChipState(_errorChip, _viewModel.ShowError);
    }

    private static void SetFilterChipState(VisualElement? chip, bool isEnabled)
    {
        if (chip == null)
        {
            return;
        }

        if (isEnabled)
        {
            chip.AddToClassList("console-level-chip-selected");
        }
        else
        {
            chip.RemoveFromClassList("console-level-chip-selected");
        }
    }

    private static void SetTabState(Button? tab, bool isSelected)
    {
        if (tab == null)
        {
            return;
        }

        if (isSelected)
        {
            tab.AddToClassList("scope-tab-selected");
        }
        else
        {
            tab.RemoveFromClassList("scope-tab-selected");
        }
    }

    private static void ApplyRow(VisualElement element, SpaceWarpConsoleEntryViewModel row)
    {
        element.dataSource = row;

        VisualElement rowElement = element.Q<VisualElement>("console-row") ?? element;
        rowElement.dataSource = row;

        foreach (string rowStyleClass in RowStyleClasses)
        {
            rowElement.RemoveFromClassList(rowStyleClass);
        }

        if (!string.IsNullOrEmpty(row.StyleClass))
        {
            rowElement.AddToClassList(row.StyleClass);
        }

        var timestamp = rowElement.Q<Label>("console-row-timestamp");
        if (timestamp != null)
        {
            timestamp.dataSource = row;
        }

        var source = rowElement.Q<Label>("console-row-source");
        if (source != null)
        {
            source.dataSource = row;
        }

        var level = rowElement.Q<Label>("console-row-level");
        if (level != null)
        {
            level.dataSource = row;
            level.style.backgroundColor = GetLevelColor(row.Level);
            level.style.color = UsesDarkText(row.Level)
                ? new StyleColor(new Color(0.08f, 0.1f, 0.12f))
                : new StyleColor(Color.white);
        }

        var message = rowElement.Q<Label>("console-row-message");
        if (message != null)
        {
            message.dataSource = row;
        }
    }

    private static VisualElement CreateActivityRow()
    {
        var row = new VisualElement { name = "cli-activity-row" };
        row.AddToClassList("cli-activity-row");

        var header = new VisualElement();
        header.AddToClassList("cli-activity-row-header");

        var timestamp = new Label { name = "cli-activity-timestamp" };
        timestamp.AddToClassList("cli-activity-timestamp");
        header.Add(timestamp);

        var source = new Label { name = "cli-activity-source" };
        source.AddToClassList("cli-activity-source");
        header.Add(source);

        var status = new Label { name = "cli-activity-status" };
        status.AddToClassList("cli-activity-status");
        header.Add(status);
        row.Add(header);

        var command = new Label { name = "cli-activity-command" };
        command.AddToClassList("cli-activity-command");
        row.Add(command);

        var payload = new Label { name = "cli-activity-payload" };
        payload.AddToClassList("cli-activity-payload");
        row.Add(payload);

        var result = new Label { name = "cli-activity-result" };
        result.AddToClassList("cli-activity-result");
        row.Add(result);
        return row;
    }

    private static VisualElement CreateLuaScriptRow()
    {
        var row = new VisualElement();
        row.AddToClassList("lua-script-row");
        var label = new Label { name = "lua-script-row-label" };
        label.AddToClassList("lua-script-row-label");
        row.Add(label);
        return row;
    }

    private static void ApplyLuaScriptRow(VisualElement element, string path)
    {
        Label label = element.Q<Label>("lua-script-row-label");
        if (label == null)
        {
            return;
        }

        label.text = path;
        label.tooltip = path;
        element.tooltip = path;
    }

    private static void ApplyActivityRow(VisualElement element, SpaceWarpConsoleCliActivityEntryViewModel row)
    {
        element.RemoveFromClassList("cli-activity-row-running");
        element.RemoveFromClassList("cli-activity-row-success");
        element.RemoveFromClassList("cli-activity-row-error");
        if (!string.IsNullOrEmpty(row.StyleClass))
        {
            element.AddToClassList(row.StyleClass);
        }

        SetLabel(element, "cli-activity-timestamp", row.Timestamp);
        SetLabel(element, "cli-activity-source", row.Source);
        SetLabel(element, "cli-activity-status", row.Status);
        SetLabel(element, "cli-activity-command", row.Command);
        SetLabel(element, "cli-activity-payload", row.Payload);
        SetLabel(element, "cli-activity-result", row.Result);
        element.tooltip = row.Tooltip;
    }

    private static void SetLabel(VisualElement root, string name, string text)
    {
        Label label = root.Q<Label>(name);
        if (label == null)
        {
            return;
        }

        label.text = text;
        label.tooltip = text;
    }

    private static Color GetLevelColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Fatal => Color.red,
            LogLevel.Error => UI.Instance.ConfigErrorColor,
            LogLevel.Warning => UI.Instance.ConfigWarningColor,
            LogLevel.Message => UI.Instance.ConfigMessageColor,
            LogLevel.Info => UI.Instance.ConfigInfoColor,
            LogLevel.Debug => UI.Instance.ConfigDebugColor,
            _ => UI.Instance.ConfigMessageColor
        };
    }

    private static bool UsesDarkText(LogLevel level)
    {
        return level is LogLevel.Warning or LogLevel.Info;
    }
}
