namespace Dixin.Linq.Lambda
{
    using System;

    public static class ChurchNumeral<T>
    {
        // Zero = f => x => x
        public static readonly Numeral<T>
            Zero = f => x => x;

        // One = f => x => f(x)
        public static readonly Numeral<T>
            One = f => x => f(x);

        // One2 = f => f ^ 1
        public static readonly Numeral<T>
            One2 = f => f;

        // Two = f => x => f(f(x))
        public static readonly Numeral<T>
            Two = f => x => f(f(x));

        // Two2 = f => f ^ 2
        public static readonly Numeral<T>
            Two2 = f => f.o(f);

        // Three = f => x => f(f(f(x)))
        public static readonly Numeral<T>
            Three = f => x => f(f(f(x)));

        // Three2 = f => f ^ 3
        public static readonly Numeral<T>
            Three2 = f => f.o(f).o(f);

        // ...
    }

    public static partial class ChurchNumeral
    {
        // Increase = n => f => x => f(n(f)(x))
        public static Numeral<T> Increase<T>(this Numeral<T> numeral) => f => x => f(numeral(f)(x));

        // Increase2 = n => f => f ^ (n + 1)
        public static Numeral<T> Increase2<T>(this Numeral<T> numeral) => f => f.o(numeral(f));

        // Add = a => b => f => x => a(f)(b(f)(x))
        public static Numeral<T> Add<T>(this Numeral<T> a, Numeral<T> b) => f => x => a(f)(b(f)(x));

        // Add2 = a => b => f => f ^ (a + b)
        public static Numeral<T> Add2<T>(this Numeral<T> a, Numeral<T> b) => f => a(f).o(b(f));

        // Add3 = a => b => a(Increase)(b)
        public static Numeral<T> Add3<T>(this Numeral<Numeral<T>> a, Numeral<T> b) => a(Increase)(b);

        // Decrease = n => f => x => n(g => h => h(g(f)))(_ => x)(_ => _)
        public static Numeral<T> Decrease<T>(this Numeral<Func<Func<T, T>, T>> numeral) =>
                    f => x => numeral(g => h => h(g(f)))(_ => x)(_ => _);

#if DEMO
        // Cannot be compiled.
        // Subtract = a => b => b(Decrease)(a)
        public static Numeral<T> Subtract<T>(Numeral<T> a, Numeral<Numeral<Func<Func<T, T>, T>>> b) => b(Decrease)(a);
#endif
    }

    // Curried from T Numeral<T>(Func<T, T> f, T x).
    // Numeral<T> is the alias for FuncFunc<T, T>, Func<T, T>>.
    public delegate Func<T, T> Numeral<T>(Func<T, T> f);

    public partial class Numeral
    {
        private readonly Numeral predecessor;

        public Numeral(Numeral predecessor)
        {
            this.predecessor = predecessor;
        }

        protected virtual Numeral Predecessor => this.predecessor;

        public virtual Func<T, T> Invoke<T>(Func<T, T> f) =>
            f.o(this.Predecessor.Invoke(f));
    }

    public partial class Numeral
    {
        private Numeral()
        {
        }

        private class ZeroNumeral : Numeral
        {
            protected override Numeral Predecessor => this;

            public override Func<T, T> Invoke<T>(Func<T, T> f) =>
                x => x;
        }

        public static Numeral Zero { get; } = new ZeroNumeral();
    }
}
