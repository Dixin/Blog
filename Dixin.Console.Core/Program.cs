namespace Dixin.Console
{
    using System.Diagnostics;
    using System.IO;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            using (TextWriterTraceListener traceListener = new TextWriterTraceListener(File.Create(Path.Combine(Path.GetTempPath(), "Trace.txt"))))
            {
                Trace.Listeners.Add(traceListener);

                Trace.Listeners.Remove(traceListener);
            }
        }
    }
}

