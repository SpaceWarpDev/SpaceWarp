using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SpaceWarp2.UI.Console;

internal static class SpaceWarpConsoleCliIntegrationBridge
{
    private const string NotLoadedMessage = "CLI integration runtime is not loaded.";

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
