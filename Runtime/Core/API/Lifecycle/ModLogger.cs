using MoonSharp.Interpreter;
using ReduxLib.Logging;

namespace SpaceWarp2.API.Lifecycle;

/// <summary>
/// The Lua <c>Log</c> global
/// </summary>
/// <remarks>
/// The runtime creates one per mod environment, wrapping that mod's existing descriptor logger, so output is
/// attributed and leveled without the mod naming anything game-side. Should be used instead of <c>print</c>.
/// A mod that wants an extra named logger gets one through <c>SW:GetLogger(name)</c>.
/// </remarks>
[MoonSharpUserData]
public class ModLogger
{
    private readonly ILogger _logger;

    static ModLogger()
    {
        UserData.RegisterType<ModLogger>();
    }

    public ModLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void Info(string message) => _logger.LogInfo(message);

    public void Warning(string message) => _logger.LogWarning(message);

    public void Error(string message) => _logger.LogError(message);

    public void Debug(string message) => _logger.LogDebug(message);
}
