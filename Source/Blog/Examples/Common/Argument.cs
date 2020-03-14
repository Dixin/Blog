namespace Examples.Common
{
    using System;
    using System.Collections.Generic;
#if NETSTANDARD2_1
    using System.Diagnostics.CodeAnalysis;
#endif
    using System.Linq;
    using System.Reflection;

    public static class Argument
    {
        public static void NotNull<T>(
#if NETSTANDARD2_1
            [NotNull]
#endif
            [ValidatedNotNull]this T value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void NotNullOrWhiteSpace(
#if NETSTANDARD2_1
            [NotNull]
#endif
            [ValidatedNotNull]this string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void NotNullOrEmpty(
#if NETSTANDARD2_1
            [NotNull]
#endif
            [ValidatedNotNull]this string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void NotNullOrEmpty<T>(
#if NETSTANDARD2_1
            [NotNull]
#endif
            [ValidatedNotNull]this IEnumerable<T> value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            if (!value.Any())
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        public static void NotNull<T>(Func<T> value)
        {
            if (value() == null)
            {
                throw new ArgumentNullException(GetName(value));
            }
        }

        private static string GetName<TValue>(Func<TValue> func)
        {
            // http://weblogs.asp.net/fredriknormen/how-to-validate-a-method-s-arguments
            FieldInfo[] fields = func.Target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return fields.Single().Name;
        }
    }
}
