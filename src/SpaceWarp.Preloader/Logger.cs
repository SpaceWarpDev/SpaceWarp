using System.ComponentModel;

namespace SpaceWarp.Preloader;

internal enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

internal static class LogLevelExtensions
{
    public static string ToLogString(this LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Debug => "DEBUG",
            LogLevel.Info => "INFO ",
            LogLevel.Warning => "WARN ",
            LogLevel.Error => "ERR  ",
            _ => throw new InvalidEnumArgumentException(nameof(logLevel), (int)logLevel, typeof(LogLevel))
        };
    }
}

internal class Logger
{
    private readonly string _logPath;

    public Logger(string gamePath)
    {
        _logPath = Path.Combine(gamePath, "BepInEx", "SpaceWarp.Preload.log");

        if (File.Exists(_logPath))
        {
            File.Delete(_logPath);
        }
    }

    private void Log(object message, LogLevel logLevel = LogLevel.Info)
    {
        var logMessage =
            $"[{logLevel.ToLogString()}: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
        File.AppendAllText(_logPath, logMessage);
    }

    public void LogDebug(object message)
    {
        Log(message, LogLevel.Debug);
    }

    public void LogInfo(object message)
    {
        Log(message, LogLevel.Info);
    }

    public void LogWarning(object message)
    {
        Log(message, LogLevel.Warning);
    }

    public void LogError(object message)
    {
        Log(message, LogLevel.Error);
    }

    public void LogException(Exception ex, string message = null)
    {
        var logMessage = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
        if (message != null)
        {
            logMessage = $"{message}{Environment.NewLine}{logMessage}";
        }

        LogError(logMessage);
    }
}