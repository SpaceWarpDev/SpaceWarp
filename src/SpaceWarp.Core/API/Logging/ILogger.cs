using JetBrains.Annotations;

namespace SpaceWarp.API.Logging;

/// <summary>
/// Interface for loggers.
/// </summary>
[PublicAPI]
public interface ILogger
{
    /// <summary>
    /// Logs the given object with the given log level.
    /// </summary>
    /// <param name="level">Log level.</param>
    /// <param name="x">Object to log.</param>
    public void Log(LogLevel level, object x);

    /// <summary>
    /// Logs the given object with the log level <see cref="LogLevel.Fatal"/>.
    /// </summary>
    /// <param name="x">Object to log.</param>
    public void LogFatal(object x);

    /// <summary>
    /// Logs the given object with the log level <see cref="LogLevel.Error"/>.
    /// </summary>
    /// <param name="x">Object to log.</param>
    public void LogError(object x);

    /// <summary>
    /// Logs the given object with the log level <see cref="LogLevel.Warning"/>.
    /// </summary>
    /// <param name="x">Object to log.</param>
    public void LogWarning(object x);

    /// <summary>
    /// Logs the given object with the log level <see cref="LogLevel.Message"/>.
    /// </summary>
    /// <param name="x">Object to log.</param>
    public void LogMessage(object x);

    /// <summary>
    /// Logs the given object with the log level <see cref="LogLevel.Info"/>.
    /// </summary>
    /// <param name="x">Object to log.</param>
    public void LogInfo(object x);

    /// <summary>
    /// Logs the given object with the log level <see cref="LogLevel.Debug"/>.
    /// </summary>
    /// <param name="x">Object to log.</param>
    public void LogDebug(object x);
}