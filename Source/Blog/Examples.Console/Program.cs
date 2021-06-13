using System;
using System.Diagnostics;
using System.IO;
using Examples.Common;

AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data");
using TextWriterTraceListener traceListener = new(Path.Combine(Path.GetTempPath(), "Trace.txt"));
Trace.Listeners.Add(traceListener);

Trace.Listeners.Remove(traceListener);
