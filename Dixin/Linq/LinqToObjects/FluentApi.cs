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

namespace Dixin.Linq.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Dixin.Linq.Fundamentals;

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
            list.ForEach(value => Trace.WriteLine(value));
            list.Clear();
        }

        internal static void FluentList()
        {
            FluentList<int> fluentlist = new FluentList<int>() { 1, 2, 3, 4, 5 }
                .Add(1)
                .Insert(0, 0)
                .RemoveAt(1)
                .Reverse()
                .ForEach(value => Trace.WriteLine(value))
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
                .FluentForEach(value => Trace.WriteLine(value))
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
                                    value => Trace.WriteLine(value))
                                );
        }

        internal static void Debug()
        {
            Person[] persons =
                {
                    new Person() { Age = 25, Name = "Anna" },
                    new Person() { Age = 30, Name = "Bob" },
                    new Person() { Age = 35, Name = "Charlie" },
                    new Person() { Age = 30, Name = "Dixin" },
                };
            LinqToObjects.FilterAndSort(persons);
        }
    }
}

#if DEMO
namespace System.Collections.Generic
{
    using System.Linq;

    public interface IEnumerable<out T> : IEnumerable
    {
        // IEnumerable:
        // IEnumerator GetEnumerator(); 

        IEnumerator<T> GetEnumerator();
    }

    public interface IQueryable<out T> : IEnumerable<T>, IQueryable, IEnumerable
    {
        // IQueryable:
        // Type ElementType { get; }
        // Expression Expression { get; }
        // IQueryProvider Provider { get; }

        // IEnumerable:
        // IEnumerator GetEnumerator(); 

        // IEnumerable:
        // IEnumerator<T> GetEnumerator();
    }
}

namespace System.Linq
{
    public static class Enumerable
    {
        public static IEnumerable<TSource> Concat<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second);

        IEnumerable<TSource> Intersect<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second);

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector);

        public static IEnumerable<TSource> Skip<TSource>(
            this IEnumerable<TSource> source, int count);

        public static IEnumerable<TSource> Take<TSource>(
            this IEnumerable<TSource> source, int count);

        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        bool Any<TSource>(this IEnumerable<TSource> source);

        int Count<TSource>(this IEnumerable<TSource> source);

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        // More query methods...
    }

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

        // Other query methods...
    }
}

namespace System.Linq
{
    public interface IOrderedEnumerable<TElement> : IEnumerable<TElement>, IEnumerable
    {
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }
}

namespace System.Linq
{
    using System.Linq.Expressions;

    public static class Queryable
    {
        public static IQueryable<TSource> Concat<TSource>(
            this IQueryable<TSource> source1, IEnumerable<TSource> source2);

        public static IQueryable<TSource> Intersect<TSource>(
            this IQueryable<TSource> source1, IEnumerable<TSource> source2);

        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector);

        public static IQueryable<TSource> Skip<TSource>(
            this IQueryable<TSource> source, int count);

        public static IQueryable<TSource> Take<TSource>(
            this IQueryable<TSource> source, int count);

        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate);
    }
}


namespace System.Linq
{
    using System.Collections;
    using System.Linq.Expressions;

    public interface IOrderedQueryable : IQueryable, IEnumerable
    {
    }

    public interface IOrderedQueryable<out T> : IQueryable<T>, IEnumerable<T>, IOrderedQueryable, IQueryable, IEnumerable
    {
    }

    public static class Queryable
    {
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
            this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);

        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey>(
            this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);

        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(
            this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);

        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);
    }
}
#endif
