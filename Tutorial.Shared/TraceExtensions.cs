namespace Tutorial
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class TraceExtensions
    {
        public static T WriteLine<T>(this T value, Func<T, string> messageFactory = null)
        {
            if (messageFactory == null)
            {
                Trace.WriteLine(value);
            }
            else
            {
                Trace.WriteLine(messageFactory(value));
            }
            return value;
        }

        public static T Write<T>(this T value, Func<T, string> messageFactory = null)
        {
            if (messageFactory == null)
            {
                Trace.Write(value);
            }
            else
            {
                Trace.Write(messageFactory(value));
            }
            return value;
        }

        public static IEnumerable<T> WriteLines<T>(this IEnumerable<T> values, Func<T, string> messageFactory = null)
        {
            foreach (T value in values)
            {
                value.WriteLine(messageFactory);
            }
            return values;
        }
    }
}
