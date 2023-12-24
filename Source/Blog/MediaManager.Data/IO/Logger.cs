namespace MediaManager.IO;

internal static class Logger
{
    internal static void WriteLine(string message = "") => Trace.WriteLine(message);
}