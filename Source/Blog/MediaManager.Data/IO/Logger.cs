namespace MediaManager.IO;

using Spectre.Console;

internal static class Logger
{
    internal static void WriteLine(string message = "") => AnsiConsole.MarkupLine(message);
}