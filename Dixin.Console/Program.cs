namespace Dixin.Console
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Dixin.Linq.Fundamentals;

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
