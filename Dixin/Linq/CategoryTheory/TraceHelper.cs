namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    using Microsoft.FSharp.Core;

    public static partial class TraceHelper
    {
        public static IO<Unit> Log
            (string log) =>
                () =>
                    {
                        Trace.WriteLine($"{DateTime.Now.ToString("o", CultureInfo.InvariantCulture)} - {log}");
                        return null;
                    };
    }
}
