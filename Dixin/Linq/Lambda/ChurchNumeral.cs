namespace Dixin.Linq.Lambda
{
    using System;

    // Curried from object Numeral(Func<object, object> f, object x).
    // Numeral is the alias for FuncFunc<object, object>, Func<object, object>>.
    public delegate Func<object, object> Numeral(Func<object, object> f);
    
    public static class ChurchNumeral<T>
    {
        // Zero = f => x => x
        public static readonly Numeral
            Zero = f => x => x;

        // One = f => x => f(x)
        public static readonly Numeral
            One = f => x => f(x);

        // One2 = f => f ^ 1
        public static readonly Numeral
            One2 = f => f;

        // Two = f => x => f(f(x))
        public static readonly Numeral
            Two = f => x => f(f(x));

        // Two2 = f => f ^ 2
        public static readonly Numeral
            Two2 = f => f.o(f);

        // Three = f => x => f(f(f(x)))
        public static readonly Numeral
            Three = f => x => f(f(f(x)));

        // Three2 = f => f ^ 3
        public static readonly Numeral
            Three2 = f => f.o(f).o(f);

        // ...
    }

    public static partial class ChurchNumeral
    {
        // Increase = n => f => x => f(n(f)(x))
        public static Numeral Increase(this Numeral numeral) => f => x => f(numeral(f)(x));

        // Increase2 = n => f => f ^ (n + 1)
        public static Numeral IncreaseWithComposition(this Numeral numeral) => f => f.o(numeral(f));

        // Add = a => b => f => x => a(f)(b(f)(x))
        public static Numeral Add(this Numeral a, Numeral b) => f => x => a(f)(b(f)(x));

        // Add2 = a => b => f => f ^ (a + b)
        public static Numeral AddWithComposition(this Numeral a, Numeral b) => f => a(f).o(b(f));

        // Add3 = a => b => a(Increase)(b)
        public static Numeral AddWithIncrease(this Numeral a, Numeral b) => (Numeral)a(x => Increase((Numeral)x))(b);

        // Decrease = n => f => x => n(g => h => h(g(f)))(_ => x)(_ => _)
        public static Numeral Decrease(this Numeral numeral) =>
            f => (x => ((Func<Func<object, object>, object>)numeral(y => new Func<Func<Func<object, object>, object>, Func<Func<object, object>, object>>(g => h => h(g(f)))((Func<Func<object, object>, object>)y))(new Func<Func<object, object>, object>(_ => x)))(_ => _));

        // Subtract = a => b => b(Decrease)(a)
        public static Numeral Subtract(this Numeral a, Numeral b) => (Numeral)b(x => Decrease((Numeral)x))(a);// Multiply = a => b => a(x => b.Add(x))(Zero)

        public static Numeral Multiply(this Numeral a, Numeral b) => (Numeral)a(n => b.Add((Numeral)n))(Zero);

        // Power = m => e => e(x => m.Multiply(x))(1)
        public static Numeral Pow(this Numeral mantissa, Numeral exponent) =>
            (Numeral)exponent(n => mantissa.Multiply((Numeral)n))(One);

        public static readonly Numeral Zero = f => x => x;

        public static readonly Numeral One = Zero.Increase();

#if DEMO
        // _DivideBy = dividend => divisor => 
        //    If(dividend.IsGreaterOrEqual(divisor))
        //        (_ => One + (dividend - divisor)._DivideBy(divisor))
        //        (_ => Zero);
        public static Numeral DivideBy(this Numeral dividend, Numeral divisor) =>
            ChurchBoolean<Numeral>.If(dividend.IsGreaterOrEqual(divisor))
                (_ => One.Add(dividend.Subtract(divisor).DivideBy(divisor)))
                (_ => Zero);
#endif
    }
}
