namespace Dixin.Linq.Lambda
{
    using System;

    // Curried from: T Numeral<T>(Func<T, T> f, T x)
    // Numeral<T> is just alias for FuncFunc<T, T>, Func<T, T>>
    public delegate Func<T, T> Numeral<T>(Func<T, T> f);

    public partial class _Numeral
    {
        public _Numeral(_Numeral predecessor)
        {
            this.Predecessor = predecessor;
        }

        protected virtual _Numeral Predecessor { get; set; }

        public virtual Numeral<T> Numeral<T>
            () =>
                f => f.o(this.Predecessor.Numeral<T>()(f));
    }

    public partial class _Numeral
    {
        private _Numeral()
        {
        }

        private class _ZeroNumeral : _Numeral
        {
            protected override _Numeral Predecessor { get { return this; } set { } }

            public override Numeral<T> Numeral<T>
                () =>
                    f => x => x;
        }

        public static _Numeral Zero { get; } = new _ZeroNumeral();
    }

    public partial class _Numeral
    {
        public _Numeral Increase
            () => new _Numeral(this);
    }
}
