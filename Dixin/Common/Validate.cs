namespace Dixin.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public static class Validate
    {
        public static Validation<TValue> Argument<TValue>
            (TValue value, string name) => new Validation<TValue>(value, name);

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [Obsolete("Use C# 6.0 nameof.")]
        public static Validation<TValue> Argument<TValue>
            (Func<TValue> argument) => new Validation<TValue>(argument(), GetName(argument));

        private static string GetName<TValue>(Func<TValue> func)
        {
            // http://weblogs.asp.net/fredriknormen/how-to-validate-a-method-s-arguments
            FieldInfo[] fields = func.Target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fields.Length != 1)
            {
                throw new NotSupportedException($"{nameof(func)} is invalid. It should be () => arg.");
            }

            return fields[0].Name;
        }
    }
}
