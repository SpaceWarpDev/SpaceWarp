using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Management;
using System.Threading;

namespace SpaceWarpRestarter;

internal class Restarter
{
    public static void Main(string[] args)
    {
        // Can only run on Windows
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            Console.WriteLine("This can only run on Windows.");
            Environment.Exit(1);
        }
        
        if (args.Length < 1)
        {
            Console.WriteLine(
                $"Usage: {Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location)}.exe " +
                $"<target process id> " +
                $"[optional arguments that will be passed to KSP2_x64.exe]"
            );
            Environment.Exit(1);
        }

        Console.WriteLine($"Args: {string.Join(" ", args)}");
        
        var targetPid = args[0];
        var processArgs = string.Join(" ", args.Length > 1 ? args.Skip(1).ToArray() : Array.Empty<string>());
        
        // Get the path to the KSP2_x64.exe
        var process = Process.GetProcessById(int.Parse(targetPid));
        var processModule = process.MainModule;
        Console.WriteLine($"Found process: {process.ProcessName} ({process.Id}) at {processModule?.FileName}");
        if (processModule != null)
        {
            var processPath = processModule.FileName;
        
            KillProc(int.Parse(targetPid));

            Console.WriteLine($"Starting process {processPath} {processArgs}");

            Thread.Sleep(1000);

            // Launch the process
            var ksp2Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = processPath,
                    Arguments = processArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            // Start the ksp2_process and detach from it
            ksp2Process.Start();
            ksp2Process.Dispose();
        } else {
            Console.WriteLine("Could not find the process.");
            Environment.Exit(1);
        }
        
        Environment.Exit(0);
    }
    
    // Will recursively kill all child processes of the given process and the process itself
    private static void KillProc(int pid)
    {
        var process = Process.GetProcessById(pid);

        var restarterProcess = Process.GetCurrentProcess();
    
        // We already guaranteed that this will only run on Windows
#pragma warning disable CA1416
        var searcher = new ManagementObjectSearcher(
            $"SELECT * " +
            $"FROM Win32_Process " +
            $"WHERE ParentProcessId={pid}"
        );

        foreach (var child in searcher.Get())
        {
            var processId = int.Parse(child["ProcessId"].ToString());
            if (processId != restarterProcess.Id)
            {
                KillProc(processId);
            }
        }

        Console.WriteLine($"Killing process {process.ProcessName} ({process.Id})");
        process.Kill();
#pragma warning restore CA1416
    }
}