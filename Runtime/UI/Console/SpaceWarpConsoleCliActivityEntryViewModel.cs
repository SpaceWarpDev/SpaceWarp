using System;

namespace SpaceWarp2.UI.Console;

internal sealed class SpaceWarpConsoleCliActivityEntryViewModel
{
    public SpaceWarpConsoleCliActivityEntryViewModel(
        string id,
        string timestamp,
        string source,
        string kind,
        string command,
        string payload,
        string status,
        string result
    )
    {
        Id = id;
        Timestamp = timestamp;
        Source = source;
        Kind = kind;
        Command = string.IsNullOrWhiteSpace(kind) ? command : $"{kind}: {command}";
        Payload = payload;
        Status = status;
        Result = result;
        Signature = string.Join('\u001f', id, status, result);
    }

    public string Id { get; }
    public string Timestamp { get; }
    public string Source { get; }
    public string Kind { get; }
    public string Command { get; }
    public string Payload { get; }
    public string Status { get; }
    public string Result { get; }
    public string Signature { get; }
    public string Tooltip => string.Join(Environment.NewLine, Command, Payload, Result);

    public string StyleClass => Status switch
    {
        "Running" => "cli-activity-row-running",
        "Success" => "cli-activity-row-success",
        "Error" => "cli-activity-row-error",
        _ => string.Empty
    };
}
