using SpaceWarp.API;
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLogger = HarmonyLib.Tools.Logger;
namespace SpaceWarp.UI;

public class SpaceWarpConsoleLogListener
{
    internal static readonly List<string> DebugMessages = new List<string>();
    internal static SpaceWarpGlobalConfiguration config = SpaceWarpGlobalConfiguration.Instance;

    public static void LogCallback(string condition, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Error:
                if (config.LogLevel >= (int)LogType.Error)
                    DebugMessages.Add($"[ERR] {condition}");
                break;
            case LogType.Assert:
                if (config.LogLevel >= (int)LogType.Assert)
                    DebugMessages.Add($"[AST] {condition}");
                break;
            case LogType.Warning:
                if (config.LogLevel >= (int)LogType.Warning)
                    DebugMessages.Add($"[WRN] {condition}");
                break;
            case LogType.Log:
                if (config.LogLevel >= (int)LogType.Log)
                    DebugMessages.Add($"[LOG] {condition}");
                break;
            case LogType.Exception:
                if (config.LogLevel >= (int)LogType.Exception)
                    DebugMessages.Add($"[EXC] {condition}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static void HarmonyLogCallback(object sender, HarmonyLogger.LogEventArgs e)
    {
        switch (e.LogChannel)
        {
            case HarmonyLogger.LogChannel.Info:
                if (config.HarmonyLogLevel >= (int)LogType.Log)
                    DebugMessages.Add($"[HINF] {e.Message}");
                break;
            case HarmonyLogger.LogChannel.Warn:
                if (config.HarmonyLogLevel >= (int)LogType.Warning)
                    DebugMessages.Add($"[HWRN] {e.Message}");
                break;
            case HarmonyLogger.LogChannel.Error:
                if (config.HarmonyLogLevel >= (int)LogType.Error)
                    DebugMessages.Add($"[HERR] {e.Message}");
                break;
            case HarmonyLogger.LogChannel.IL:
                if (config.HarmonyLogLevel >= (int)LogType.Exception)
                    DebugMessages.Add($"[HIL] {e.Message}");
                break;
            default:
                DebugMessages.Add($"[HARM] {e.Message}");
                break;
        }
    }
}