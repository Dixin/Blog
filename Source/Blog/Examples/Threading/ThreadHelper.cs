namespace Examples.Threading
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Examples.Runtime.InteropServices;

    public static class ThreadHelper
    {
        public static void AssignThreadToCpu
            (IntPtr thread, int cpuIndex) => NativeMethods.SetThreadAffinityMask(thread, new IntPtr(1 << cpuIndex));

        public static void AssignCurrentThreadToCpu
            (int cpuIndex) => AssignThreadToCpu(NativeMethods.GetCurrentThread(), cpuIndex);

        public static void Sta(Action action, bool ignoreException = false) => Sta<object?>(
            () =>
                {
                    action();
                    return null;
                },
            ignoreException);

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [return: MaybeNull]
        public static TResult Sta<TResult>(Func<TResult> func, bool ignoreException = false)
        {
            Exception? staThreadException = null;
            TResult result = default;
            Thread staThread = new Thread(() =>
            {
                try
                {
                    result = func();
                }
                catch (Exception exception)
                {
                    staThreadException = exception;
                }
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            if (!ignoreException && staThreadException != null)
            {
                throw staThreadException;
            }

            return result;
        }
    }
}
