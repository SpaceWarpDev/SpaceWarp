using System;
using ReduxLib.Logging;
using UitkForKsp2.MVVM.Core;
using Unity.Properties;

namespace SpaceWarp2.UI.Console;

internal sealed class SpaceWarpConsoleEntryViewModel : ViewModelBase
{
    private SpaceWarpConsoleEntryViewModel(
        string timestamp,
        string levelText,
        string source,
        string message,
        string styleClass,
        LogLevel level
    )
    {
        Timestamp = timestamp;
        LevelText = levelText;
        Source = source;
        Message = message;
        StyleClass = styleClass;
        Level = level;
    }

    [CreateProperty] public string Timestamp { get; }
    [CreateProperty] public string LevelText { get; }
    [CreateProperty] public string Source { get; }
    [CreateProperty] public string Message { get; }
    [CreateProperty] public string StyleClass { get; }
    public LogLevel Level { get; }

    public static SpaceWarpConsoleEntryViewModel FromLogInfo(SpaceWarpConsoleLogListener.LogInfo info)
    {
        string timestamp = UI.Instance.ConfigShowTimeStamps
            ? info.DateTime.ToString(UI.Instance.ConfigTimeStampFormat)
            : string.Empty;
        return new SpaceWarpConsoleEntryViewModel(
            timestamp,
            info.Level.AsString(),
            info.Source?.Name ?? "Unknown",
            info.Data?.ToString() ?? string.Empty,
            GetStyleClass(info.Level),
            info.Level
        );
    }

    public bool MatchesSearch(string searchText)
    {
        return Contains(Source, searchText) ||
               Contains(Message, searchText) ||
               Contains(LevelText, searchText);
    }

    private static bool Contains(string source, string searchText)
    {
        return source?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string GetStyleClass(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => "console-row-debug",
            LogLevel.Info => "console-row-info",
            LogLevel.Message => "console-row-message-level",
            LogLevel.Warning => "console-row-warning",
            LogLevel.Error => "console-row-error",
            LogLevel.Fatal => "console-row-fatal",
            _ => string.Empty
        };
    }
}
