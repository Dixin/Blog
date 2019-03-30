namespace Tutorial.Functional
{
    using System;

    public interface ILocal
    {
        ILocal<T> Cast<T>();
    }

    public interface ILocal<T> : ILocal
    {
        ILocal<T> Where(Func<T, bool> predicate);

        ILocal<TResult> Select<TResult>(Func<T, TResult> selector);

        ILocal<TResult> SelectMany<TSelector, TResult>(
            Func<T, ILocal<TSelector>> selector,
            Func<T, TSelector, TResult> resultSelector);

        ILocal<TResult> Join<TInner, TKey, TResult>(
            ILocal<TInner> inner,
            Func<T, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<T, TInner, TResult> resultSelector);

        ILocal<TResult> GroupJoin<TInner, TKey, TResult>(
            ILocal<TInner> inner,
            Func<T, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<T, ILocal<TInner>, TResult> resultSelector);

        IOrderedLocal<T> OrderBy<TKey>(Func<T, TKey> keySelector);

        IOrderedLocal<T> OrderByDescending<TKey>(Func<T, TKey> keySelector);

        ILocal<ILocalGroup<TKey, T>> GroupBy<TKey>(Func<T, TKey> keySelector);

        ILocal<ILocalGroup<TKey, TElement>> GroupBy<TKey, TElement>(
            Func<T, TKey> keySelector, Func<T, TElement> elementSelector);
    }

    public interface IOrderedLocal<T> : ILocal<T>
    {
        IOrderedLocal<T> ThenBy<TKey>(Func<T, TKey> keySelector);

        IOrderedLocal<T> ThenByDescending<TKey>(Func<T, TKey> keySelector);
    }

    public interface ILocalGroup<TKey, T> : ILocal<T>
    {
        TKey Key { get; }
    }
}

namespace Tutorial.Functional
{
    using System;
    using System.Linq.Expressions;

    public interface IRemote
    {
        IRemote<T> Cast<T>();
    }

    public interface IRemote<T> : IRemote
    {
        IRemote<T> Where(Expression<Func<T, bool>> predicate);

        IRemote<TResult> Select<TResult>(Expression<Func<T, TResult>> selector);

        IRemote<TResult> SelectMany<TSelector, TResult>(
            Expression<Func<T, IRemote<TSelector>>> selector,
            Expression<Func<T, TSelector, TResult>> resultSelector);

        IRemote<TResult> Join<TInner, TKey, TResult>(
            IRemote<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<T, TInner, TResult>> resultSelector);

        IRemote<TResult> GroupJoin<TInner, TKey, TResult>(
            IRemote<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<T, IRemote<TInner>, TResult>> resultSelector);

        IOrderedRemote<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);

        IOrderedRemote<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

        IRemote<IRemoteGroup<TKey, T>> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector);

        IRemote<IRemoteGroup<TKey, TElement>> GroupBy<TKey, TElement>(
            Expression<Func<T, TKey>> keySelector, Expression<Func<T, TElement>> elementSelector);
    }

    public interface IOrderedRemote<T> : IRemote<T>
    {
        IOrderedRemote<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);

        IOrderedRemote<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
    }

    public interface IRemoteGroup<TKey, T> : IRemote<T>
    {
        TKey Key { get; }
    }
}
