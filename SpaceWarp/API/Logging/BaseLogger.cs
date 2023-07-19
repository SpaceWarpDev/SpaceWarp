namespace SpaceWarp.API.Logging;

public abstract class BaseLogger : ILogger
{
    public abstract void Log(LogLevel level, object x);
    public void LogNone(object x) => Log(LogLevel.None, x);

    public void LogFatal(object x) => Log(LogLevel.Fatal, x);

    public void LogError(object x) => Log(LogLevel.Error, x);

    public void LogWarning(object x) => Log(LogLevel.Warning, x);

    public void LogMessage(object x) => Log(LogLevel.Message, x);

    public void LogInfo(object x) => Log(LogLevel.Info, x);

    public void LogDebug(object x) => Log(LogLevel.Debug, x);

    public void LogAll(object x) => Log(LogLevel.All, x);
}