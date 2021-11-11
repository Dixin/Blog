namespace Examples.Runtime.InteropServices;

using System.Runtime.InteropServices;

public static class NativeMethods
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern int GetCurrentThreadId();

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr SetThreadAffinityMask(
        IntPtr hThread,
        IntPtr dwThreadAffinityMask);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr GetCurrentThread();
}