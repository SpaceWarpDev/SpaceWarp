using MoonSharp.Interpreter;
using ReduxLib.Logging;

namespace SpaceWarp2.API.Lifecycle;

/// <summary>
/// The Lua <c>Log</c> global.
/// </summary>
/// <remarks>
/// The runtime creates one per mod environment, wrapping that mod's existing descriptor logger, so output is
/// attributed and leveled without the mod naming anything game-side. Should be used instead of <c>print</c>.
/// A mod that wants an extra named logger gets one through <c>SW:GetLogger(name)</c>.
/// </remarks>
[MoonSharpUserData]
public sealed class ModLogger
{
    private readonly ILogger _logger;

    static ModLogger()
    {
        UserData.RegisterType<ModLogger>();
    }

    /// <summary>
    /// Creates a logger wrapping the given ReduxLib logger.
    /// </summary>
    /// <param name="logger">The underlying logger to forward to.</param>
    public ModLogger(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs an info-level message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Info(string message) => _logger.LogInfo(message);

    /// <summary>
    /// Logs a warning-level message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Warning(string message) => _logger.LogWarning(message);

    /// <summary>
    /// Logs an error-level message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Error(string message) => _logger.LogError(message);

    /// <summary>
    /// Logs a debug-level message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Debug(string message) => _logger.LogDebug(message);
}
