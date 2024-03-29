using System.Diagnostics;

if (args.Length < 1)
{
    Console.WriteLine(
        $"Usage: {Path.GetFileNameWithoutExtension(AppContext.BaseDirectory)} <path to KSP2_x64.exe> " +
        $"[optional arguments]"
    );
    Environment.Exit(1);
}

var processPath = args[0];
var processName = Path.GetFileNameWithoutExtension(processPath);
var processArgs = args.Length > 1 ? args[1..] : Array.Empty<string>();

Console.WriteLine($"Waiting for {processName} to exit...");

while (true)
{
    if (Process.GetProcessesByName(processName).Length == 0)
    {
        try
        {
            Console.WriteLine($"{processName}.exe is not running. Attempting to start the process...");
            Process.Start(processPath, processArgs);
            Console.WriteLine($"{processName}.exe started successfully.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while starting {processName}.exe: {ex.Message}");
            Environment.Exit(1);
        }
    }
    else
    {
        Thread.Sleep(500);
    }
}