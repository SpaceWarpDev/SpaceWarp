using System;
using System.Text;

namespace SpaceWarp.API.Logging;

/// <summary>
/// Unique logger for each mod, each mod has its own logger to accomodate different behaviours.
/// </summary>
public class ModLogger : BaseModLogger
{
    private readonly string _moduleName;

    /// <summary>
    /// Creates a ModLogger for a module
    /// </summary>
    /// <param name="moduleName"></param>
    public ModLogger(string moduleName)
    {
        _moduleName = moduleName;
    }
        
    private string BuildLogMessage(LogLevel level, string message)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"[{DateTime.Now:HH:mm:ss.fff}] ");
        sb.Append($"[{_moduleName}] ");
        sb.Append($"[{level}] ");
        sb.Append(message);

        return sb.ToString();
    }

    protected override void Log(LogLevel level, string message)
    {
        if ((int)level >= SpaceWarpGlobalConfiguration.Instance.LogLevel)
        {
            string logMessage = BuildLogMessage(level, message);
            UnityEngine.Debug.Log(logMessage);
        }
    }
}