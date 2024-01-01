using JetBrains.Annotations;

namespace SpaceWarp.API.Logging;

/// <summary>
/// Base class for loggers.
/// </summary>
[PublicAPI]
public abstract class BaseLogger : ILogger
{
    /// <inheritdoc />
    public abstract void Log(LogLevel level, object x);

    /// <inheritdoc />
    public void LogFatal(object x) => Log(LogLevel.Fatal, x);

    /// <inheritdoc />
    public void LogError(object x) => Log(LogLevel.Error, x);

    /// <inheritdoc />
    public void LogWarning(object x) => Log(LogLevel.Warning, x);

    /// <inheritdoc />
    public void LogMessage(object x) => Log(LogLevel.Message, x);

    /// <inheritdoc />
    public void LogInfo(object x) => Log(LogLevel.Info, x);

    /// <inheritdoc />
    public void LogDebug(object x) => Log(LogLevel.Debug, x);
}