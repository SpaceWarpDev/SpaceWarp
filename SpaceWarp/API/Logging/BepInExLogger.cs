using BepInEx.Logging;

namespace SpaceWarp.API.Logging;

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
}