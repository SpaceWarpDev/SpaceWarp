namespace SpaceWarp.API.Logging
{
    public interface IModLogger
    {
        void Log(LogLevel level, string message);
        
    }
}