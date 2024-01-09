using System.Runtime.InteropServices;
using UnityEngine;

namespace SpaceWarp.Backend.Processes;

internal static class ProcessStarter
{
    private const uint CreateNewConsole = 0x00000010;

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StartupInfo
    {
        public uint cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }
    
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CreateProcess(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        [In] ref StartupInfo lpStartupInfo,
        out ProcessInformation lpProcessInformation
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    public static bool StartDetachedProcess(string applicationName, string commandLine)
    {
        var si = new StartupInfo();
        si.cb = (uint)Marshal.SizeOf(si);

        var success = CreateProcess(
            applicationName,
            commandLine,
            IntPtr.Zero,
            IntPtr.Zero,
            false,
            CreateNewConsole,
            IntPtr.Zero,
            null,
            ref si,
            out var pi
        );

        if (!success)
        {
            return false;
        }

        // Close the handles to the process and thread
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);

        return true;
    }
}