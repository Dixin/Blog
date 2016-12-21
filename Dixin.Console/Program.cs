namespace Dixin.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using Dixin.Common;
    using Dixin.IO;
    using Dixin.Linq.CSharp;

    internal static class Program
    {
        private static void Main()
        {
            AppDomainData.DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data");
            using (TextWriterTraceListener traceListener = new TextWriterTraceListener(Path.Combine(Path.GetTempPath(), "Trace.txt")))
            {
                IEnumerable<string> first = new string[] { null, string.Empty, "ss", };
                IEnumerable<string> second = new string[] { null, string.Empty, "ß", };
                CultureInfo.CurrentCulture = new CultureInfo("en-US");
                bool sequentialEqual1 = first.SequenceEqual(second, StringComparer.CurrentCulture); // True
                bool sequentialEqual2 = first.SequenceEqual(second, StringComparer.Ordinal); // False
                //Trace.Listeners.Add(traceListener);
                Purity.PureFunction(@"D:\Dixin\Desktop\New folder (3)\Contracts\.NETFramework\v4.6");
                //Trace.Listeners.Remove(traceListener);
            }
        }
    }
}
