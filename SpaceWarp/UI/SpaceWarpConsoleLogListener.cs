using System;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceWarp.UI
{
    public class SpaceWarpConsoleLogListener
    {
        internal static readonly List<string> DebugMessages = new List<string>();

        public static void LogCallback(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
            case LogType.Error:
                DebugMessages.Add($"[ERR] {condition}");
                break;
            case LogType.Assert:
                DebugMessages.Add($"[AST] {condition}");
                break;
            case LogType.Warning:
                DebugMessages.Add($"[WRN] {condition}");
                break;
            case LogType.Log:
                DebugMessages.Add($"[LOG] {condition}");
                break;
            case LogType.Exception:
                DebugMessages.Add($"[EXC] {condition}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}