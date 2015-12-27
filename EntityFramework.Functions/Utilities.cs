namespace EntityFramework.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal static class StringExtensions
    {
        internal static bool EqualsOrdinal
                (this string a, string b) => string.Equals(a, b, StringComparison.Ordinal);
    }

    internal static class EnumerableExtensions
    {
        internal static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> next)
        {
            foreach (TSource value in source)
            {
                next(value);
            }
        }
    }

#if NET40
    internal static class CustomAttributeExtensions
    {
        internal static T GetCustomAttribute<T>(this MemberInfo element) where T : Attribute =>
            (T)element.GetCustomAttribute(typeof(T));

        internal static Attribute GetCustomAttribute(this MemberInfo element, Type attributeType) =>
            Attribute.GetCustomAttribute(element, attributeType);

        internal static T GetCustomAttribute<T>(this ParameterInfo element) where T : Attribute =>
            (T)element.GetCustomAttribute(typeof(T));

        internal static Attribute GetCustomAttribute(this ParameterInfo element, Type attributeType) =>
            Attribute.GetCustomAttribute(element, attributeType);

        internal static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element) where T : Attribute =>
            (IEnumerable<T>)element.GetCustomAttributes(typeof(T));

        internal static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, Type attributeType) =>
            Attribute.GetCustomAttributes(element, attributeType);
    }
#endif
}