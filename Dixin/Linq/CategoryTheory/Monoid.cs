namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Diagnostics.Contracts;

    public partial interface IMonoid<T>
    {
        T Unit {[Pure] get; }

        Func<T, T, T> Binary {[Pure] get; }
    }

    public partial class Monoid<T> : IMonoid<T>
    {
        public Monoid(T unit, [Pure] Func<T, T, T> binary)
        {
            this.Unit = unit;
            this.Binary = binary;
        }

        public T Unit {[Pure] get; }

        public Func<T, T, T> Binary {[Pure] get; }
    }

    [Pure]
    public static partial class MonoidExtensions
    {
        public static IMonoid<T> Monoid<T>
            (this T unit, Func<T, T, T> binary) => new Monoid<T>(unit, binary);

        public static IMonoid<Nullable<TSource>> MonoidOfNullable<TSource>
            (this IMonoid<TSource> monoid) =>
                new Monoid<Nullable<TSource>>(
                    new Nullable<TSource>(),
                    (a, b) => new Nullable<TSource>(() =>
                        {
                            if (a.HasValue && b.HasValue)
                            {
                                return Tuple.Create(true, monoid.Binary(a.Value, b.Value));
                            }

                            if (a.HasValue)
                            {
                                return Tuple.Create(true, a.Value);
                            }

                            if (b.HasValue)
                            {
                                return Tuple.Create(true, b.Value);
                            }

                            return Tuple.Create(false, default(TSource));
                        }));
    }
}
