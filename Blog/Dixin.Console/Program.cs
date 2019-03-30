﻿namespace Dixin.Console
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using Dixin.Common;

    internal static class Program
    {
        private static void Main()
        {
            AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data");
            using (TextWriterTraceListener traceListener = new TextWriterTraceListener(Path.Combine(Path.GetTempPath(), "Trace.txt")))
            {
                Trace.Listeners.Add(traceListener);

                Trace.Listeners.Remove(traceListener);
            }
        }
    }
}
