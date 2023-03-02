using System.Collections.Generic;
using BepInEx.Logging;

namespace SpaceWarp.UI;

public sealed class SpaceWarpConsoleLogListener : ILogListener
{
    internal static readonly List<string> DebugMessages = new();
    
    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        DebugMessages.Add(eventArgs.ToStringLine());
    }

    public void Dispose()
    {
        DebugMessages.Clear();
    }
}
