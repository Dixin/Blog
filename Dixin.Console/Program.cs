namespace Dixin.Console
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using Dixin.Common;

    internal static class Program
    {
        private static void Main()
        {
            AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data");
            using (TextWriterTraceListener traceListener = new TextWriterTraceListener(Path.Combine(Path.GetTempPath(), "Trace.txt")))
            {
                Trace.Listeners.Add(traceListener);
                Thread.Sleep(TimeSpan.FromSeconds(2));

                Trace.Listeners.Remove(traceListener);
            }
        }
    }
}
