namespace Dixin.Common
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    public static class Argument
    {
        public static void Requires(bool condition, string message, string paramName = null)
        {
            if (!condition)
            {
                throw new ArgumentException(message, paramName);
            }
            
            Contract.EndContractBlock();
        }

        public static void Range(bool condition, string message, string paramName = null)
        {
            if (!condition)
            {
                throw new ArgumentOutOfRangeException(paramName, message);
            }

            Contract.EndContractBlock();
        }

        public static void NotNull<T>([ValidatedNotNull]this T value, string name = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            Contract.EndContractBlock();
        }

        public static void NotNullOrWhiteSpace([ValidatedNotNull]this string value, string name = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(name);
            }

            Contract.EndContractBlock();
        }

        public static void NotNullOrEmpty([ValidatedNotNull]this string value, string name = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(name);
            }

            Contract.EndContractBlock();
        }

        public static void NotNull<T>(Func<T> value)
        {
            if (value() == null)
            {
                throw new ArgumentNullException(GetName(value));
            }

            Contract.EndContractBlock();
        }

        private static string GetName<TValue>(Func<TValue> func)
        {
            // http://weblogs.asp.net/fredriknormen/how-to-validate-a-method-s-arguments
            FieldInfo[] fields = func.Target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return fields.Length == 1 ? fields[0].Name : null;
        }
    }
}
