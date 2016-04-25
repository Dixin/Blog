namespace Dixin.Console
{
    using System.Diagnostics;

    internal static class Program
    {
        private static void Main()
        {
            using (TextWriterTraceListener traceListener = new TextWriterTraceListener(@"D:\Temp\Trace.txt"))
            {
                Trace.Listeners.Add(traceListener);
            }
        }
    }
}
