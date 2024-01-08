using System.Diagnostics;
using System.Reflection;
using System.Management;

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

var targetPid = args[0];
var processArgs = string.Join(" ", args.Length > 1 ? args[1..] : Array.Empty<string>());

// Get the path to the KSP2_x64.exe
var processModule = Process.GetProcessById(int.Parse(targetPid)).MainModule;
if (processModule != null)
{
    var processPath = processModule.FileName;

    KillProc(int.Parse(targetPid));

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

// Will recursively kill all child processes of the given process and the process itself
void KillProc(int pid)
{
    var process = Process.GetProcessById(pid);
    
    // We already guaranteed that this will only run on Windows
#pragma warning disable CA1416
    var searcher = new ManagementObjectSearcher(
        $"SELECT * " +
        $"FROM Win32_Process " +
        $"WHERE ParentProcessId={pid}"
    );
    
    foreach (var child in searcher.Get())
    {
        KillProc(int.Parse(child["ProcessId"].ToString() ?? throw new InvalidOperationException()));
    }
    
    process.Kill();
#pragma warning restore CA1416
}