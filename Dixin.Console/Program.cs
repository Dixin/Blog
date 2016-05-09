namespace Dixin.Console
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    internal static class Program
    {
        private static void Main()
        {
            using (TextWriterTraceListener traceListener = new TextWriterTraceListener(@"D:\Temp\Trace.txt"))
            {
                Trace.Listeners.Add(traceListener);
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }
}
