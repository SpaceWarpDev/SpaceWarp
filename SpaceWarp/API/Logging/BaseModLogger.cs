namespace SpaceWarp.API.Logging
{
    /// <summary>
    /// Used to print logs on each mod.
    /// </summary>
    public abstract class BaseModLogger
    {
        protected abstract void Log(LogLevel level, string message, params object[] args);

        public void Trace(string message, params object[] args) => Log(LogLevel.Trace, message, args);
        public void Debug(string message, params object[] args) => Log(LogLevel.Debug, message, args);
        public void Info(string message, params object[] args) => Log(LogLevel.Info, message, args);
        public void Warn(string message, params object[] args) => Log(LogLevel.Warn, message, args);
        public void Error(string message, params object[] args) => Log(LogLevel.Error, message, args);
        public void Critical(string message, params object[] args) => Log(LogLevel.Critical, message, args);
    }
}