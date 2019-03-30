namespace Tutorial
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class TraceExtensions
    {
        public static T WriteLine<T>(this T value)
        {
            Trace.WriteLine(value);
            return value;
        }

        public static T Write<T>(this T value)
        {
            Trace.Write(value);
            return value;
        }

        public static IEnumerable<T> WriteLines<T>(this IEnumerable<T> values, Func<T, string> messageFactory = null)
        {
            if (messageFactory != null)
            {
                foreach (T value in values)
                {
                    string message = messageFactory(value);
                    Trace.WriteLine(message);
                }
            }
            else
            {
                foreach (T value in values)
                {
                    Trace.WriteLine(value);
                }
            }
            return values;
        }
    }
}
