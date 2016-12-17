namespace Dixin.Linq
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public static class TraceExtensions
    {
        public static T TraceLine<T>(this T value, Func<T, string> messageFactory = null)
        {
            messageFactory = messageFactory ?? (_ => value?.ToString());
            System.Diagnostics.Trace.WriteLine(messageFactory(value));
            return value;
        }

        public static T Trace<T>(this T value, Func<T, string> messageFactory = null)
        {
            messageFactory = messageFactory ?? (_ => value?.ToString());
            System.Diagnostics.Trace.Write(messageFactory(value));
            return value;
        }

        public static IEnumerable<T> ForEachTraceLine<T>(this IEnumerable<T> values, Func<T, string> messageFactory = null)
        {
            values.ForEach(value => value.TraceLine(messageFactory));
            return values;
        }

        public static IEnumerable<T> ForEachTrace<T>(this IEnumerable<T> values, Func<T, string> messageFactory = null)
        {
            values.ForEach(value => value.Trace(messageFactory));
            return values;
        }
    }
}
