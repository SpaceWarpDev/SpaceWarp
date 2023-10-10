using BepInEx.Logging;
using JetBrains.Annotations;

namespace SpaceWarp.API.Logging;

[PublicAPI]
public class BepInExLogger : BaseLogger
{
    private ManualLogSource _log;

    public BepInExLogger(ManualLogSource log)
    {
        _log = log;
    }

    public override void Log(LogLevel level, object x)
    {
        _log.Log((BepInEx.Logging.LogLevel)level, x);
    }

    public static implicit operator BepInExLogger(ManualLogSource log) => new(log);
}