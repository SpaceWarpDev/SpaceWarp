using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SpaceWarp2.API;
using SpaceWarp2.API.Lifecycle;

namespace SpaceWarp2.UI.Console;

internal static class SpaceWarpConsoleLuaService
{
    private const string ConsoleLuaOutputPrefix = "[Lua:spacewarp-console] ";

    private static bool _luaOutputSubscribed;
    private static ConsoleScriptRun? _activeRun;
    private static string _activeLuaRunId = string.Empty;
    private static string _activeLuaCompletionText = string.Empty;
    private static bool _activeLuaErrored;

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _luaOutputSubscribed = false;
        _activeRun = null;
        _activeLuaRunId = string.Empty;
        _activeLuaCompletionText = string.Empty;
        _activeLuaErrored = false;
    }

    public static event Action<string>? LuaOutputReceived;

    public static bool IsLuaRunning => IsLuaRunActive();

    public static SpaceWarpConsoleCSharpResult RunLua(string code)
    {
        EnsureLuaOutputSubscription();
        if (IsLuaRunActive())
        {
            return SpaceWarpConsoleCSharpResult.Failure("A Lua script is already running.");
        }

        string runId = string.Empty;
        try
        {
            runId = BeginLuaScriptRun();
            SetCurrentLuaScriptRun(runId);
            _activeLuaRunId = runId;
            _activeRun = ModScriptRuntime.RunConsole(code);
            _activeRun.Resume();
            return CompleteLuaIfFinished("Started. Script.Log output appears on the Logs tab.");
        }
        catch (Exception ex)
        {
            CancelLuaScriptRun(_activeLuaRunId);
            ClearActiveLuaRun();
            return SpaceWarpConsoleCSharpResult.Failure(ex.GetBaseException().Message);
        }
        finally
        {
            ClearCurrentLuaScriptRun(runId);
        }
    }

    public static SpaceWarpConsoleCSharpResult TickLua()
    {
        if (!IsLuaRunActive())
        {
            return SpaceWarpConsoleCSharpResult.FromSuccess(string.Empty);
        }

        if (_activeRun != null && !_activeRun.IsFinished)
        {
            string runId = _activeLuaRunId;
            try
            {
                SetCurrentLuaScriptRun(runId);
                _activeRun.Resume();
            }
            finally
            {
                ClearCurrentLuaScriptRun(runId);
            }
        }

        return CompleteLuaIfFinished(string.Empty);
    }

    public static SpaceWarpConsoleCSharpResult StopLua()
    {
        if (!IsLuaRunActive())
        {
            return SpaceWarpConsoleCSharpResult.Failure("No Lua script is running.");
        }

        CancelLuaScriptRun(_activeLuaRunId);
        ClearActiveLuaRun();
        return SpaceWarpConsoleCSharpResult.FromSuccess("Stopped.");
    }

    public static IReadOnlyList<string> ListLuaScripts()
    {
        try
        {
            string root = GetLuaScriptRoot();
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            return Directory
                .EnumerateFiles(root, "*.lua", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static SpaceWarpConsoleCSharpResult ReadLuaScript(string relativePath)
    {
        try
        {
            string path = ResolveLuaScriptPath(relativePath, mustBeLuaFile: true);
            return File.Exists(path)
                ? SpaceWarpConsoleCSharpResult.FromSuccess(File.ReadAllText(path))
                : SpaceWarpConsoleCSharpResult.Failure("Lua script file does not exist.");
        }
        catch (Exception ex)
        {
            return SpaceWarpConsoleCSharpResult.Failure(ex.GetBaseException().Message);
        }
    }

    public static SpaceWarpConsoleCSharpResult WriteLuaScript(string relativePath, string text)
    {
        try
        {
            string path = ResolveLuaScriptPath(relativePath, mustBeLuaFile: true);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetLuaScriptRoot());
            File.WriteAllText(path, text ?? string.Empty);
            return SpaceWarpConsoleCSharpResult.FromSuccess("Saved " + NormalizeLuaRelativePath(relativePath) + ".");
        }
        catch (Exception ex)
        {
            return SpaceWarpConsoleCSharpResult.Failure(ex.GetBaseException().Message);
        }
    }

    public static SpaceWarpConsoleCSharpResult CreateLuaScript()
    {
        try
        {
            string root = GetLuaScriptRoot();
            Directory.CreateDirectory(root);
            for (int index = 0; index < 1000; index++)
            {
                string relativePath = index == 0 ? "new_script.lua" : $"new_script_{index}.lua";
                string path = ResolveLuaScriptPath(relativePath, mustBeLuaFile: true);
                if (File.Exists(path))
                {
                    continue;
                }

                File.WriteAllText(path, "-- New Lua script\n");
                return SpaceWarpConsoleCSharpResult.FromSuccess(relativePath);
            }

            return SpaceWarpConsoleCSharpResult.Failure("Could not create a unique Lua script name.");
        }
        catch (Exception ex)
        {
            return SpaceWarpConsoleCSharpResult.Failure(ex.GetBaseException().Message);
        }
    }

    public static SpaceWarpConsoleCSharpResult OpenLuaScriptFolder()
    {
        try
        {
            string root = GetLuaScriptRoot();
            Directory.CreateDirectory(root);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(root)
            {
                UseShellExecute = true
            });
            return SpaceWarpConsoleCSharpResult.FromSuccess(root);
        }
        catch (Exception ex)
        {
            return SpaceWarpConsoleCSharpResult.Failure(ex.GetBaseException().Message);
        }
    }

    private static SpaceWarpConsoleCSharpResult CompleteLuaIfFinished(string runningMessage)
    {
        if (_activeRun != null && _activeRun.IsFinished)
        {
            _activeLuaErrored = _activeRun.IsErrored;
            _activeLuaCompletionText = FormatLuaResult(_activeRun.Result);
            _activeRun = null;
            CompleteLuaScriptRun(_activeLuaRunId);
        }

        if (IsLuaRunActive())
        {
            return SpaceWarpConsoleCSharpResult.FromSuccess(runningMessage);
        }

        string resultText = string.IsNullOrWhiteSpace(_activeLuaCompletionText)
            ? "Completed."
            : _activeLuaCompletionText;
        bool success = !_activeLuaErrored;
        ClearActiveLuaRun();
        return success
            ? SpaceWarpConsoleCSharpResult.FromSuccess(resultText)
            : SpaceWarpConsoleCSharpResult.Failure(resultText);
    }

    private static string FormatLuaResult(string result)
    {
        return string.Equals(result, "nil", StringComparison.OrdinalIgnoreCase) ? string.Empty : result;
    }

    private static bool IsLuaRunActive()
    {
        if (_activeRun != null)
        {
            return true;
        }

        return !string.IsNullOrEmpty(_activeLuaRunId) && IsLuaScriptRunActive(_activeLuaRunId);
    }

    private static void ClearActiveLuaRun()
    {
        _activeRun = null;
        _activeLuaRunId = string.Empty;
        _activeLuaCompletionText = string.Empty;
        _activeLuaErrored = false;
    }

    private static string BeginLuaScriptRun()
    {
        Type? utilityType = FindType("KSP.ScriptInterop.LuaScriptUtilityMgr");
        MethodInfo? method = utilityType?.GetMethod("BeginScriptRun", BindingFlags.Public | BindingFlags.Static);
        return method?.Invoke(null, new object[] { "spacewarp-console", "SpaceWarp Console", "UI" })?.ToString() ?? string.Empty;
    }

    private static void SetCurrentLuaScriptRun(string runId)
    {
        InvokeLuaUtility("SetCurrentScriptRun", runId);
    }

    private static void ClearCurrentLuaScriptRun(string runId)
    {
        InvokeLuaUtility("ClearCurrentScriptRun", runId);
    }

    private static void CompleteLuaScriptRun(string runId)
    {
        InvokeLuaUtility("CompleteScriptRun", runId);
    }

    private static void CancelLuaScriptRun(string runId)
    {
        InvokeLuaUtility("CancelScriptRun", runId);
    }

    private static bool IsLuaScriptRunActive(string runId)
    {
        Type? utilityType = FindType("KSP.ScriptInterop.LuaScriptUtilityMgr");
        MethodInfo? method = utilityType?.GetMethod("IsScriptRunActive", BindingFlags.Public | BindingFlags.Static);
        return method?.Invoke(null, new object[] { runId }) is bool active && active;
    }

    private static void InvokeLuaUtility(string methodName, string runId)
    {
        if (string.IsNullOrEmpty(runId))
        {
            return;
        }

        Type? utilityType = FindType("KSP.ScriptInterop.LuaScriptUtilityMgr");
        MethodInfo? method = utilityType?.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        method?.Invoke(null, new object[] { runId });
    }

    private static string GetLuaScriptRoot()
    {
        return Path.GetFullPath(CommonPaths.LuaFolder);
    }

    private static string ResolveLuaScriptPath(string relativePath, bool mustBeLuaFile)
    {
        relativePath = NormalizeLuaRelativePath(relativePath);
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Lua script path is required.");
        }

        if (Path.IsPathRooted(relativePath))
        {
            throw new InvalidOperationException("Lua script path must be relative to Lua.");
        }

        if (mustBeLuaFile && !relativePath.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Lua script path must end with .lua.");
        }

        string root = GetLuaScriptRoot();
        string fullPath = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        string rootWithSeparator = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Lua script path is outside Lua.");
        }

        return fullPath;
    }

    private static string NormalizeLuaRelativePath(string relativePath)
    {
        return (relativePath ?? string.Empty).Trim().Replace('\\', '/');
    }

    private static void EnsureLuaOutputSubscription()
    {
        if (_luaOutputSubscribed)
        {
            return;
        }

        Type? utilityType = FindType("KSP.ScriptInterop.LuaScriptUtilityMgr");
        EventInfo? outputEvent = utilityType?.GetEvent("OutputLogged", BindingFlags.Public | BindingFlags.Static);
        if (outputEvent == null)
        {
            return;
        }

        outputEvent.AddEventHandler(null, (Action<string>)OnLuaOutputLogged);
        _luaOutputSubscribed = true;
    }

    private static void OnLuaOutputLogged(string message)
    {
        LuaOutputReceived?.Invoke(TrimConsoleLuaOutputPrefix(message));
    }

    private static string TrimConsoleLuaOutputPrefix(string message)
    {
        return message.StartsWith(ConsoleLuaOutputPrefix, StringComparison.Ordinal)
            ? message[ConsoleLuaOutputPrefix.Length..]
            : message;
    }

    private static Type? FindType(string fullName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? type = assembly.GetType(fullName, false);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }
}
