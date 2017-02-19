namespace Tutorial.Tests.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class LinqHelper
    {
        internal static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate) => Enumerable.Where(source, predicate);

        internal static TSource[] ToArray<TSource>(
            this IEnumerable<TSource> source) => Enumerable.ToArray(source);

        internal static bool Any<TSource>(
            this IEnumerable<TSource> source) => Enumerable.Any(source);

        internal static TSource Single<TSource>(
            this IEnumerable<TSource> source) => Enumerable.Single(source);

        internal static TSource Last<TSource>(
            this IEnumerable<TSource> source) => Enumerable.Last(source);

        internal static IEnumerable<TSource> Concat<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second) => Enumerable.Concat(first, second);

        internal static IEnumerable<TSource> Skip<TSource>(
            this IEnumerable<TSource> source, int count) => Enumerable.Skip(source, count);
    }
}