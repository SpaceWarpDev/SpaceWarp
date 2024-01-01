using JetBrains.Annotations;

namespace SpaceWarp.API.Logging;

[PublicAPI]
public abstract class BaseLogger : ILogger
{
    public abstract void Log(LogLevel level, object x);

    public void LogFatal(object x) => Log(LogLevel.Fatal, x);

    public void LogError(object x) => Log(LogLevel.Error, x);

    public void LogWarning(object x) => Log(LogLevel.Warning, x);

    public void LogMessage(object x) => Log(LogLevel.Message, x);

    public void LogInfo(object x) => Log(LogLevel.Info, x);

    public void LogDebug(object x) => Log(LogLevel.Debug, x);
}