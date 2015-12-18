namespace Dixin.Threading
{
    using System;

    using Dixin.Runtime.InteropServices;

    public static class ThreadHelper
    {
        public static void AssignThreadToCpu
            (IntPtr thread, int cpuIndex) => NativeMethods.SetThreadAffinityMask(thread, new IntPtr(1 << cpuIndex));

        public static void AssignCurrentThreadToCpu
            (int cpuIndex) => AssignThreadToCpu(NativeMethods.GetCurrentThread(), cpuIndex);
    }
}
