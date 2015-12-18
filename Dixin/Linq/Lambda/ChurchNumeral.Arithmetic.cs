namespace Dixin.Linq.Lambda
{
    using System;

    public static partial class _NumeralExtensions
    {
        // Increase = n => n.Increase()
        private static _Numeral Increase
            (_Numeral numeral) => numeral.Increase();

        // Add = a => b => a(Increase)(b)
        public static _Numeral Add
            (this _Numeral a, _Numeral b) => a.Numeral<_Numeral>()(Increase)(b);
    }

    public static partial class _NumeralExtensions
    {
        public static _Numeral Zero { get; } = _Numeral.Zero;

        public static _Numeral One { get; } = _Numeral.Zero.Increase();

        // ...

        // Decrease = n => f => x => n(g => h => h(g(f)))(_ => x)(_ => _)
        public static _Numeral Decrease
            (this _Numeral numeral) =>
                new Numeral<_Numeral>(f => x =>
                    numeral.Numeral<Func<Func<_Numeral, _Numeral>, _Numeral>>()(g => h => h(g(f)))(_ => x)(_ => _))
                    (Increase)(Zero);

        // Subtract = a => b => b(Decrease)(a)
        public static _Numeral Subtract
            (this _Numeral a, _Numeral b) => b.Numeral<_Numeral>()(Decrease)(a);
    }

    public static partial class _NumeralExtensions
    {
        // Multiply = a => b => a(x => b.Add(x))(Zero)
        public static _Numeral Multiply
                (this _Numeral a, _Numeral b) => a.Numeral<_Numeral>()(b.Add)(Zero);

        // Power = m => e => e(x => m.Multiply(x))(1)
        public static _Numeral Pow
            (this _Numeral mantissa, _Numeral exponent) => exponent.Numeral<_Numeral>()(mantissa.Multiply)(One);
    }

    public partial class _Numeral
    {
        public static _Numeral operator +
            (_Numeral a, _Numeral b) => a.Add(b);

        public static _Numeral operator -
            (_Numeral a, _Numeral b) => a.Subtract(b);

        public static _Numeral operator *
            (_Numeral a, _Numeral b) => a.Multiply(b);

        public static _Numeral operator ^
            (_Numeral a, _Numeral b) => a.Pow(b);

        public static _Numeral operator ++
            (_Numeral numeral) => numeral.Increase();

        public static _Numeral operator --
            (_Numeral numeral) => numeral.Decrease();
    }

    public partial class _Numeral
    {
        public static _Numeral operator /
            (_Numeral a, _Numeral b) => a._DivideBy(b);
    }
}
