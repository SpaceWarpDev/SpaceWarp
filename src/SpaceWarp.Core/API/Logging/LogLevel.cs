using JetBrains.Annotations;

namespace SpaceWarp.API.Logging;

/// <summary>
/// The log level.
/// </summary>
[PublicAPI]
public enum LogLevel
{
    /// <summary>
    /// No logging.
    /// </summary>
    None = 0,
    /// <summary>
    /// Fatal errors.
    /// </summary>
    Fatal = 1,
    /// <summary>
    /// Errors.
    /// </summary>
    Error = 2,
    /// <summary>
    /// Warnings.
    /// </summary>
    Warning = 4,
    /// <summary>
    /// Messages.
    /// </summary>
    Message = 8,
    /// <summary>
    /// Information.
    /// </summary>
    Info = 16,
    /// <summary>
    /// Debug information.
    /// </summary>
    Debug = 32,
    /// <summary>
    /// All logging.
    /// </summary>
    All = Debug | Info | Message | Warning | Error | Fatal
}