#if DEMO
namespace System.Collections.Generic
{
    public class List<T> : IList<T>, IList, IReadOnlyList<T>
    {
        public void Add(T item) { /* ... */ }

        public void Clear() { /* ... */ }

        public void ForEach(Action<T> action) { /* ... */ }

        public void Insert(int index, T item) { /* ... */ }

        public void RemoveAt(int index) { /* ... */ }

        public void Reverse() { /* ... */ }

        // Other members.
    }
}
#endif

namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;

    internal class FluentList<T> : List<T>
    {
        internal new FluentList<T> Add(T item) { base.Add(item); return this; }

        internal new FluentList<T> Clear() { base.Clear(); return this; }

        internal new FluentList<T> ForEach(Action<T> action) { base.ForEach(action); return this; }

        internal new FluentList<T> Insert(int index, T item) { base.Insert(index, item); return this; }

        internal new FluentList<T> RemoveAt(int index) { base.RemoveAt(index); return this; }

        internal new FluentList<T> Reverse() { base.Reverse(); return this; }
    }

    internal static class ListExtensions
    {
        internal static List<T> FluentAdd<T>(this List<T> list, T item)
        {
            list.Add(item);
            return list;
        }

        internal static List<T> FluentClear<T>(this List<T> list)
        {
            list.Clear();
            return list;
        }

        internal static List<T> FluentForEach<T>(this List<T> list, Action<T> action)
        {
            list.ForEach(action);
            return list;
        }

        internal static List<T> FluentInsert<T>(this List<T> list, int index, T item)
        {
            list.Insert(index, item);
            return list;
        }

        internal static List<T> FluentRemoveAt<T>(this List<T> list, int index)
        {
            list.RemoveAt(index);
            return list;
        }

        internal static List<T> FluentReverse<T>(this List<T> list)
        {
            list.Reverse();
            return list;
        }
    }

    internal static partial class Fluent
    {
        internal static void List()
        {
            List<int> list = new List<int>() { 1, 2, 3, 4, 5 };
            list.Add(1);
            list.Insert(0, 0);
            list.RemoveAt(1);
            list.Reverse();
            list.ForEach(value => value.WriteLine());
            list.Clear();
        }

        internal static void FluentList()
        {
            FluentList<int> fluentlist = new FluentList<int>() { 1, 2, 3, 4, 5 }
                .Add(1)
                .Insert(0, 0)
                .RemoveAt(1)
                .Reverse()
                .ForEach(value => value.WriteLine())
                .Clear();
        }

        internal static void String()
        {
            string source = "...";
            string result = source
                .Trim()
                .Replace('a', 'b')
                .Substring(1)
                .Remove(2)
                .Insert(0, "c")
                .ToUpperInvariant();
        }

        internal static void ListFluentExtensions()
        {
            List<int> list = new List<int>() { 1, 2, 3, 4, 5 }
                .FluentAdd(1)
                .FluentInsert(0, 0)
                .FluentRemoveAt(1)
                .FluentReverse()
                .FluentForEach(value => value.WriteLine())
                .FluentClear();
        }

        internal static void CompiledListExtensions()
        {
            List<int> list = ListExtensions.FluentClear(
                                ListExtensions.FluentForEach(
                                    ListExtensions.FluentReverse(
                                        ListExtensions.FluentRemoveAt(
                                            ListExtensions.FluentInsert(
                                                ListExtensions.FluentAdd(
                                                    new List<int>() { 1, 2, 3, 4, 5 },
                                                    1),
                                                0, 0),
                                            1)
                                        ),
                                    value => value.WriteLine())
                                );
        }
    }
}

#if DEMO
namespace System.Collections
{
    public interface IEnumerable
    {
        IEnumerator GetEnumerator();
    }
}

namespace System.Collections.Generic
{
    public interface IEnumerable<out T> : IEnumerable
    {
        IEnumerator<T> GetEnumerator();
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector);

        public static IEnumerable<TSource> Concat<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second);

        // Other members.
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static TSource First<TSource>(this IEnumerable<TSource> source);

        public static TSource Last<TSource>(this IEnumerable<TSource> source);
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector);
    }
}

namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IOrderedEnumerable<TElement> : IEnumerable<TElement>, IEnumerable
    {
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }
}

#endif
