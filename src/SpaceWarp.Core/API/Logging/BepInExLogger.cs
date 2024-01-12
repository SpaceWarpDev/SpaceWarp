using BepInEx.Logging;
using JetBrains.Annotations;

namespace SpaceWarp.API.Logging;

/// <summary>
/// A logger that uses BepInEx's logging system.
/// </summary>
[PublicAPI]
public class BepInExLogger : BaseLogger
{
    private ManualLogSource _log;

    /// <summary>
    /// Creates a new instance of <see cref="BepInExLogger"/>.
    /// </summary>
    /// <param name="log">The <see cref="ManualLogSource"/> to use.</param>
    public BepInExLogger(ManualLogSource log)
    {
        _log = log;
    }

    /// <inheritdoc/>
    public override void Log(LogLevel level, object x)
    {
        _log.Log((BepInEx.Logging.LogLevel)level, x);
    }

    /// <summary>
    /// Implicitly converts a <see cref="ManualLogSource"/> to a <see cref="BepInExLogger"/>.
    /// </summary>
    /// <param name="log">The <see cref="ManualLogSource"/> to convert.</param>
    /// <returns>The converted <see cref="BepInExLogger"/>.</returns>
    public static implicit operator BepInExLogger(ManualLogSource log) => new(log);
}