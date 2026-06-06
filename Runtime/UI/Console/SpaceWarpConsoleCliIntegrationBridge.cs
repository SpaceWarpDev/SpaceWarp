using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using SpaceWarp2.API;

namespace SpaceWarp2.UI.Console;

internal static class SpaceWarpConsoleCliIntegrationBridge
{
    private const string NotLoadedMessage = "CLI integration runtime is not loaded.";
    private static bool _luaOutputSubscribed;

    public static event Action<string>? LuaOutputReceived;

    public static IReadOnlyList<SpaceWarpConsoleCliActivityEntryViewModel> GetActivityEntries()
    {
        Type? activityLogType = FindType("Redux.CliIntegration.CliIntegrationActivityLog");
        MethodInfo? snapshotMethod = activityLogType?.GetMethod(
            "Snapshot",
            BindingFlags.Public | BindingFlags.Static
        );
        if (snapshotMethod == null)
        {
            return Array.Empty<SpaceWarpConsoleCliActivityEntryViewModel>();
        }

        object? snapshot = snapshotMethod.Invoke(null, null);
        if (snapshot is not IEnumerable entries)
        {
            return Array.Empty<SpaceWarpConsoleCliActivityEntryViewModel>();
        }

        return entries
            .Cast<object>()
            .Reverse()
            .Select(ToActivityViewModel)
            .ToList();
    }

    public static object? AddActivity(string source, string kind, string command, string payload)
    {
        Type? activityLogType = FindType("Redux.CliIntegration.CliIntegrationActivityLog");
        MethodInfo? addMethod = activityLogType?.GetMethod(
            "AddStarted",
            BindingFlags.Public | BindingFlags.Static
        );
        return addMethod?.Invoke(null, new object[] { source, kind, command, payload });
    }

    public static void CompleteActivity(object? activity, bool success, string result)
    {
        if (activity == null)
        {
            return;
        }

        Type? activityLogType = FindType("Redux.CliIntegration.CliIntegrationActivityLog");
        MethodInfo? completeMethod = activityLogType?.GetMethod(
            "Complete",
            BindingFlags.Public | BindingFlags.Static
        );
        completeMethod?.Invoke(null, new[] { activity, success, result });
    }

    public static SpaceWarpConsoleCSharpResult EvaluateCSharp(string code, int depth)
    {
        Type? replType = FindType("Redux.CliIntegration.CliIntegrationCSharpRepl");
        if (replType == null)
        {
            return SpaceWarpConsoleCSharpResult.Failure(NotLoadedMessage);
        }

        object? shared = replType.GetProperty("Shared", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        MethodInfo? evaluateMethod = replType.GetMethod(
            "Evaluate",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(string), typeof(int) },
            null
        );
        if (shared == null || evaluateMethod == null)
        {
            return SpaceWarpConsoleCSharpResult.Failure("C# evaluator is not available.");
        }

        try
        {
            object? result = evaluateMethod.Invoke(shared, new object[] { code, depth });
            return SpaceWarpConsoleCSharpResult.FromSuccess(FormatCSharpResult(result));
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            return SpaceWarpConsoleCSharpResult.Failure(ex.InnerException.GetBaseException().Message);
        }
        catch (Exception ex)
        {
            return SpaceWarpConsoleCSharpResult.Failure(ex.GetBaseException().Message);
        }
    }

    public static void ResetCSharp()
    {
        Type? replType = FindType("Redux.CliIntegration.CliIntegrationCSharpRepl");
        MethodInfo? resetMethod = replType?.GetMethod("ResetShared", BindingFlags.Public | BindingFlags.Static);
        resetMethod?.Invoke(null, null);
    }

    public static SpaceWarpConsoleCSharpResult RunLua(string code)
    {
        EnsureLuaOutputSubscription();
        Type? gameManagerType = FindType("KSP.Game.GameManager");
        object? gameManager = gameManagerType
            ?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
            ?.GetValue(null);
        object? game = gameManager
            ?.GetType()
            .GetProperty("Game", BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(gameManager);
        object? scriptEnvironment = game
            ?.GetType()
            .GetProperty("ScriptEnvironment", BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(game);
        object? runInterop = scriptEnvironment
            ?.GetType()
            .GetProperty("RunInterop", BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(scriptEnvironment);
        MethodInfo? runMethod = runInterop
            ?.GetType()
            .GetMethod(
                "RunScript",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(string), typeof(string) },
                null
            );
        if (runMethod == null)
        {
            return SpaceWarpConsoleCSharpResult.Failure("Game script environment is not ready.");
        }

        try
        {
            object? result = runMethod.Invoke(runInterop, new object[] { code, "SpaceWarpConsole.lua" });
            string resultText = result?.ToString() ?? string.Empty;
            return SpaceWarpConsoleCSharpResult.FromSuccess(
                string.IsNullOrWhiteSpace(resultText) || string.Equals(resultText, "nil", StringComparison.OrdinalIgnoreCase)
                    ? "Started. Script.Log output appears on the Logs tab."
                    : resultText
            );
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            return SpaceWarpConsoleCSharpResult.Failure(ex.InnerException.GetBaseException().Message);
        }
        catch (Exception ex)
        {
            return SpaceWarpConsoleCSharpResult.Failure(ex.GetBaseException().Message);
        }
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

    private static SpaceWarpConsoleCliActivityEntryViewModel ToActivityViewModel(object entry)
    {
        return new SpaceWarpConsoleCliActivityEntryViewModel(
            GetString(entry, "Id"),
            FormatTimestamp(GetValue(entry, "StartedUtc")),
            GetString(entry, "Source"),
            GetString(entry, "Kind"),
            GetString(entry, "Command"),
            Trim(GetString(entry, "Payload"), 260),
            GetString(entry, "Status"),
            Trim(GetString(entry, "Result"), 260)
        );
    }

    private static string FormatCSharpResult(object? result)
    {
        if (result == null)
        {
            return "Completed.";
        }

        string text = GetString(result, "Text");
        if (!string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        object? value = GetValue(result, "Value");
        string valueText = value?.ToString() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(valueText) && valueText != "null")
        {
            return valueText;
        }

        string output = GetString(result, "Output");
        if (!string.IsNullOrWhiteSpace(output))
        {
            return output;
        }

        string kind = GetString(result, "Kind");
        return kind == "compile" ? "Compiled." : "Completed.";
    }

    private static object? GetValue(object target, string propertyName)
    {
        return target.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(target);
    }

    private static string GetString(object target, string propertyName)
    {
        return GetValue(target, propertyName)?.ToString() ?? string.Empty;
    }

    private static string FormatTimestamp(object? value)
    {
        return value switch
        {
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToLocalTime().ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
            DateTime dateTime => dateTime.ToLocalTime().ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
            _ => string.Empty
        };
    }

    private static string Trim(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
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
        LuaOutputReceived?.Invoke(message);
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

internal readonly struct SpaceWarpConsoleCSharpResult
{
    private SpaceWarpConsoleCSharpResult(bool success, string displayText)
    {
        Success = success;
        DisplayText = displayText;
    }

    public bool Success { get; }
    public string DisplayText { get; }

    public static SpaceWarpConsoleCSharpResult FromSuccess(string displayText)
    {
        return new SpaceWarpConsoleCSharpResult(true, displayText);
    }

    public static SpaceWarpConsoleCSharpResult Failure(string displayText)
    {
        return new SpaceWarpConsoleCSharpResult(false, displayText);
    }
}
