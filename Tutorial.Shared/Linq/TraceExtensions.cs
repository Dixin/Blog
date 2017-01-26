namespace Dixin.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static class TraceExtensions
    {
        public static T WriteLine<T>(this T value, Func<T, string> messageFactory = null)
        {
            messageFactory = messageFactory ?? (_ => value?.ToString());
            Trace.WriteLine(messageFactory(value));
            return value;
        }

        public static T Write<T>(this T value, Func<T, string> messageFactory = null)
        {
            messageFactory = messageFactory ?? (_ => value?.ToString());
            Trace.Write(messageFactory(value));
            return value;
        }

        public static IEnumerable<T> WriteLines<T>(this IEnumerable<T> values, Func<T, string> messageFactory = null)
        {
            values.ForEach(value => value.WriteLine(messageFactory));
            return values;
        }
    }
}
