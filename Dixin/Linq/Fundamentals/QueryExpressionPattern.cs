namespace Dixin.Linq.Fundamentals
{
    using System;

    public abstract class Source
    {
        public abstract Source<T> Cast<T>();
    }

    public abstract class Source<T> : Source
    {
        public abstract Source<T> Where(Func<T, bool> predicate);

        public abstract Source<TResult> Select<TResult>(Func<T, TResult> selector);

        public abstract Source<TResult> SelectMany<TSelector, TResult>(
            Func<T, Source<TSelector>> selector,
            Func<T, TSelector, TResult> resultSelector);

        public abstract Source<TResult> Join<TInner, TKey, TResult>(
            Source<TInner> inner,
            Func<T, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<T, TInner, TResult> resultSelector);

        public abstract Source<TResult> GroupJoin<TInner, TKey, TResult>(
            Source<TInner> inner,
            Func<T, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<T, Source<TInner>, TResult> resultSelector);

        public abstract OrderedSource<T> OrderBy<TKey>(Func<T, TKey> keySelector);

        public abstract OrderedSource<T> OrderByDescending<TKey>(Func<T, TKey> keySelector);

        public abstract Source<SoourceGroup<TKey, T>> GroupBy<TKey>(Func<T, TKey> keySelector);

        public abstract Source<SoourceGroup<TKey, TElement>> GroupBy<TKey, TElement>(
            Func<T, TKey> keySelector,
            Func<T, TElement> elementSelector);
    }

    public abstract class OrderedSource<T> : Source<T>
    {
        public abstract OrderedSource<T> ThenBy<TKey>(Func<T, TKey> keySelector);

        public abstract OrderedSource<T> ThenByDescending<TKey>(Func<T, TKey> keySelector);
    }

    public abstract class SoourceGroup<TKey, T> : Source<T>
    {
        public abstract TKey Key { get; }
    }
}
