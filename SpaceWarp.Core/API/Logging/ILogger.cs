namespace SpaceWarp.API.Logging;

public interface ILogger
{
    public void Log(LogLevel level, object x);

    public void LogNone(object x);
    public void LogFatal(object x);
    public void LogError(object x);
    public void LogWarning(object x);
    public void LogMessage(object x);
    public void LogInfo(object x);
    public void LogDebug(object x);
    public void LogAll(object x);

}