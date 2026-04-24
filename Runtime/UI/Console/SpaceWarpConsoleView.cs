using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using LogLevel = ReduxLib.Logging.LogLevel;

namespace SpaceWarp2.UI.Console;

internal sealed class SpaceWarpConsoleView
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
    private ListView? _logList;
    private Label? _emptyState;
    private VisualElement? _debugChip;
    private VisualElement? _infoChip;
    private VisualElement? _messageChip;
    private VisualElement? _warningChip;
    private VisualElement? _errorChip;

    public SpaceWarpConsoleView(UIDocument window, SpaceWarpConsoleViewModel viewModel)
    {
        _window = window;
        _viewModel = viewModel;
        _viewModel.EntriesChanged += RefreshEntries;
    }

    public bool IsOpen => _window.rootVisualElement.style.display != DisplayStyle.None;

    public void Load()
    {
        _window.rootVisualElement.dataSource = _viewModel;
        _window.EnableLocalization();
        CacheVisualElements();
        ConfigureListView();
        RefreshEntries();
        Hide();
        _root?.CenterByDefault();
    }

    public void Show()
    {
        _window.Show();
    }

    public void Hide()
    {
        _window.Hide();
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
        _logList = _root?.Q<ListView>("console-list");
        _emptyState = _root?.Q<Label>("empty-state");
        _debugChip = _root?.Q<VisualElement>("toggle-debug");
        _infoChip = _root?.Q<VisualElement>("toggle-info");
        _messageChip = _root?.Q<VisualElement>("toggle-message");
        _warningChip = _root?.Q<VisualElement>("toggle-warning");
        _errorChip = _root?.Q<VisualElement>("toggle-error");
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

    private static Color GetLevelColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Fatal => Color.red,
            LogLevel.Error => UI.Instance.ConfigErrorColor.Value,
            LogLevel.Warning => UI.Instance.ConfigWarningColor.Value,
            LogLevel.Message => UI.Instance.ConfigMessageColor.Value,
            LogLevel.Info => UI.Instance.ConfigInfoColor.Value,
            LogLevel.Debug => UI.Instance.ConfigDebugColor.Value,
            _ => UI.Instance.ConfigMessageColor.Value
        };
    }

    private static bool UsesDarkText(LogLevel level)
    {
        return level is LogLevel.Warning or LogLevel.Info;
    }
}
