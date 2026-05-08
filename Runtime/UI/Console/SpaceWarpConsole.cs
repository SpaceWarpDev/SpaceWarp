using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceWarp2.UI.Console;

[RequireComponent(typeof(UIDocument))]
internal sealed class SpaceWarpConsole : MonoBehaviour
{
    private SpaceWarpConsoleViewModel? _viewModel;
    private SpaceWarpConsoleView? _view;

    private void Start()
    {
        var document = GetComponent<UIDocument>();
        _viewModel = new SpaceWarpConsoleViewModel();
        _view = new SpaceWarpConsoleView(document, _viewModel);
        _view.Load();

        _viewModel.InitializeFromLogs(SpaceWarpConsoleLogListener.LogMessages);
        _viewModel.CloseRequested += HideWindow;
        _viewModel.ClearRequested += ClearLogs;
        SpaceWarpConsoleLogListener.OnNewLog += OnNewLog;
    }

    private void OnDestroy()
    {
        SpaceWarpConsoleLogListener.OnNewLog -= OnNewLog;
        _view?.Dispose();
        if (_viewModel == null)
        {
            return;
        }

        _viewModel.CloseRequested -= HideWindow;
        _viewModel.ClearRequested -= ClearLogs;
    }

    private void Update()
    {
        if (_view == null)
        {
            return;
        }

        if (_view.IsOpen)
        {
            _viewModel?.PollCliIntegrationActivity();
        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C))
        {
            ToggleWindow();
        }

        if (_view.IsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            HideWindow();
        }
    }

    public void Show()
    {
        _view?.Show();
        _view?.ScrollToBottom();
    }

    public void Hide()
    {
        HideWindow();
    }

    private void ToggleWindow()
    {
        if (_view == null)
        {
            return;
        }

        if (_view.IsOpen)
        {
            HideWindow();
            return;
        }

        Show();
    }

    private void HideWindow()
    {
        _view?.Hide();
    }

    private void ClearLogs()
    {
        UI.Instance.SpaceWarpConsoleLogListener!.Clear();
        _viewModel?.ClearLogs();
    }

    private void OnNewLog(SpaceWarpConsoleLogListener.LogInfo info)
    {
        if (_viewModel == null)
        {
            return;
        }

        _viewModel.AddLog(info);
        if (_view is { IsOpen: true } && _viewModel.AutoScroll)
        {
            _view.ScrollToBottom();
        }
    }
}
