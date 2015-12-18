namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Diagnostics.Contracts;

    public interface ICategory<TCategory> where TCategory : ICategory<TCategory>
    {
        // o = (m2, m1) -> composition
        [Pure]
        IMorphism<TSource, TResult, TCategory> o<TSource, TMiddle, TResult>(
            IMorphism<TMiddle, TResult, TCategory> m2, IMorphism<TSource, TMiddle, TCategory> m1);

        [Pure]
        IMorphism<TObject, TObject, TCategory> Id<TObject>();
    }

    public interface IMorphism<in TSource, out TResult, out TCategory> where TCategory : ICategory<TCategory>
    {
        [Pure]
        TCategory Category { get; }

        [Pure]
        TResult Invoke(TSource source);
    }

    public class DotNet : ICategory<DotNet>
    {
        [Pure]
        public IMorphism<TObject, TObject, DotNet> Id<TObject>
            () => new DotNetMorphism<TObject, TObject>(@object => @object);

        [Pure]
        public IMorphism<TSource, TResult, DotNet> o<TSource, TMiddle, TResult>
            (IMorphism<TMiddle, TResult, DotNet> m2, IMorphism<TSource, TMiddle, DotNet> m1) =>
                new DotNetMorphism<TSource, TResult>(@object => m2.Invoke(m1.Invoke(@object)));

        private DotNet()
        {
        }

        public static DotNet Category {[Pure] get; } = new DotNet();
    }

    public class DotNetMorphism<TSource, TResult> : IMorphism<TSource, TResult, DotNet>
    {
        private readonly Func<TSource, TResult> function;

        public DotNetMorphism(Func<TSource, TResult> function)
        {
            this.function = function;
        }

        public DotNet Category
        {
            [Pure]get {return DotNet.Category;}
        }

        [Pure]
        public TResult Invoke
            (TSource source) => this.function(source);
    }

    [Pure]
    public static class MorphismExtensions
    {
        public static IMorphism<TSource, TResult, DotNet> o<TSource, TMiddle, TResult>(
            this IMorphism<TMiddle, TResult, DotNet> m2, IMorphism<TSource, TMiddle, DotNet> m1)
        {
            Contract.Requires<ArgumentException>(m2.Category == m1.Category, "m2 and m1 are not in the same category.");

            return m1.Category.o(m2, m1);
        }

        public static IMorphism<TSource, TResult, DotNet> DotNetMorphism<TSource, TResult>
            (this Func<TSource, TResult> function) => new DotNetMorphism<TSource, TResult>(function);
    }

    #region Monoid

    public partial interface IMonoid<T> : ICategory<IMonoid<T>>
    {
    }

    public class MonoidMorphism<T> : IMorphism<T, T, IMonoid<T>>
    {
        private readonly Func<T, T> function;

        public MonoidMorphism(IMonoid<T> category, Func<T, T> function)
        {
            this.function = function;
            this.Category = category;
        }

        public IMonoid<T> Category {[Pure] get; }

        [Pure]
        public T Invoke
            (T source) => this.function(source);
    }

    public partial class Monoid<T>
    {
        [Pure]
        public IMorphism<TSource, TResult, IMonoid<T>> o<TSource, TMiddle, TResult>(
            IMorphism<TMiddle, TResult, IMonoid<T>> m2, IMorphism<TSource, TMiddle, IMonoid<T>> m1)
        {
            if (!(typeof(T).IsAssignableFrom(typeof(TSource)) && typeof(T).IsAssignableFrom(typeof(TMiddle))
                && typeof(T).IsAssignableFrom(typeof(TResult))))
            {
                throw new InvalidOperationException($"Category {nameof(Monoid<T>)} has only 1 object {nameof(T)}.");
            }

            return new MonoidMorphism<T>(
                this,
                _ => this.Binary(
                    (T)(object)m1.Invoke((TSource)(object)this.Unit),
                    (T)(object)m2.Invoke((TMiddle)(object)this.Unit)))
                as IMorphism<TSource, TResult, IMonoid<T>>;
        }

        [Pure]
        public IMorphism<TObject, TObject, IMonoid<T>> Id<TObject>()
        {
            if (!typeof(T).IsAssignableFrom(typeof(TObject)))
            {
                throw new InvalidOperationException($"Category {nameof(Monoid<T>)} has only 1 object {nameof(T)}.");
            }

            return new MonoidMorphism<T>(this, value => value) as IMorphism<TObject, TObject, IMonoid<T>>;
        }
    }

    public static partial class MonoidExtensions
    {
        public static IMorphism<T, T, IMonoid<T>> MonoidMorphism<T>
            (this IMonoid<T> category, Func<T, T> function) => new MonoidMorphism<T>(category, function);
    }

    #endregion
}
