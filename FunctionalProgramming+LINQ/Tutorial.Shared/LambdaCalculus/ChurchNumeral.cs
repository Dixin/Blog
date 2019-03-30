namespace Tutorial.LambdaCalculus
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using static ChurchBoolean;
    using static ChurchNumeral;

    public static partial class CompiledChurchNumeral
    {
        public static readonly Func<Numeral, Func<Numeral, Numeral>> DivideBySelfReference;

        static CompiledChurchNumeral()
        {
            DivideBySelfReference = dividend => divisor =>
                If(dividend.IsGreaterThanOrEqualTo(divisor))
                    (_ => One.Add(DivideBySelfReference(dividend.Subtract(divisor))(divisor)))
                    (_ => Zero);
        }
    }

    // Curried from (dynamic -> dynamic, dynamic) -> dynamic.
    // Numeral is the alias of (dynamic -> dynamic) -> dynamic -> dynamic.
    public delegate Func<dynamic, dynamic> Numeral(Func<dynamic, dynamic> f);

    public static partial class ChurchNumeral
    {
        public static readonly Numeral
            Zero = f => x => x;

        public static readonly Numeral
            One = f => x => f(x);

        public static readonly Numeral
            Two = f => x => f(f(x));

        public static readonly Numeral
            Three = f => x => f(f(f(x)));

        // ...
    }

    public static partial class ChurchNumeral
    {
        // One = f => f
        public static readonly Numeral
            OneWithComposition = f => f;

        // Two = f => f o f
        public static readonly Numeral
            TwoWithComposition = f => f.o(f);

        // Three = f => f o f o f
        public static readonly Numeral
            ThreeWithComposition = f => f.o(f).o(f);

        // ...
    }

    public static partial class ChurchNumeral
    {
        public static readonly Func<Numeral, Numeral> Increase = n => f => x => f(n(f)(x));

        public static readonly Func<Numeral, Numeral> IncreaseWithComposition = n => f => f.o(n(f));

        // Decrease = n => f => x => n(g => h => h(g(f)))(_ => x)(Id)
        public static readonly Func<Numeral, Numeral> Decrease =
            n =>
                f =>
                    x =>
                        n(g => new Func<Func<dynamic, dynamic>, dynamic>(h => h(g(f))))(
                            new Func<Func<dynamic, dynamic>, dynamic>(_ => x))(
                            new Func<dynamic, dynamic>(Functions<dynamic>.Id));

        // Add = a => b => f => x => a(f)(b(f)(x))
        public static readonly Func<Numeral, Func<Numeral, Numeral>> Add = a => b => f => x => b(f)(a(f)(x));

        // Add = a => b => f => f ^ (a + b)
        public static readonly Func<Numeral, Func<Numeral, Numeral>> AddWithComposition = a => b => f => a(f).o(b(f));

#if DEMO
// Add = a => b => b(Increase)(a)
        public static readonly Func<Numeral, Func<Numeral, Numeral>>
            AddWithIncrease = a => b => (Numeral)b(Increase)(a);
#endif

        // Add = a => b => b(Increase)(a)
        // η conversion:
        // Add = a => b => b(n => Increase(n))(a)
        public static readonly Func<Numeral, Func<Numeral, Numeral>> AddWithIncrease = a => b => b(n => Increase(n))(a);

        // Subtract = a => b => b(Decrease)(a)
        // η conversion:
        // Subtract = a => b => b(n => Decrease(n))(a)
        public static readonly Func<Numeral, Func<Numeral, Numeral>> Subtract = a => b => b(n => Decrease(n))(a);

        // Multiply = a => b => b(Add(a))(a)
        // η conversion:
        // Multiply = a => b => b(n => Add(a)(n))(Zero)
        public static readonly Func<Numeral, Func<Numeral, Numeral>> Multiply = a => b => b(n => Add(a)(n))(Zero);

        // Pow = a => b => b(Multiply(a))(a)
        // η conversion:
        // Pow = a => b => b(n => Multiply(a)(n))(1)
        public static readonly Func<Numeral, Func<Numeral, Numeral>> Pow = a => b => b(n => Multiply(a)(n))(One);
    }

    public static partial class ChurchNumeral
    {
        // DivideBySelfReference = dividend => divisor => 
        //    If(dividend >= divisor)
        //        (_ => 1 + DivideBySelfReference(dividend - divisor)(divisor))
        //        (_ => 0);
        public static readonly Func<Numeral, Func<Numeral, Numeral>>
            DivideBySelfReference = dividend => divisor =>
                If(dividend.IsGreaterThanOrEqualTo(divisor))
                    (_ => One.Add(DivideBySelfReference(dividend.Subtract(divisor))(divisor)))
                    (_ => Zero);
    }

    public static partial class ChurchNumeral
    {
        public static Func<Numeral, Numeral> DivideByMethod(Numeral dividend) => divisor =>
            If(dividend.IsGreaterThanOrEqualTo(divisor))
                (_ => One.Add(DivideByMethod(dividend.Subtract(divisor))(divisor)))
                (_ => Zero);
    }

#if DEMO
        internal static void Inline()
        {
            Func<Numeral, Func<Numeral, Numeral>> divideBy = dividend => divisor =>
                If(dividend.IsGreaterThanOrEqualTo(divisor))
                    (_ => One.Add(divideBy(dividend.Subtract(divisor))(divisor)))
                    (_ => Zero);
        }
#endif
    public static partial class ChurchNumeral
    {
        internal static void Inline()
        {
            Func<Numeral, Func<Numeral, Numeral>> divideBy = null;
            divideBy = dividend => divisor =>
                If(dividend.IsGreaterThanOrEqualTo(divisor))
                    (_ => One.Add(divideBy(dividend.Subtract(divisor))(divisor)))
                    (_ => Zero);
        }

        // Decrease = n => n(tuple => tuple.Shift(Increase))(0, 0).Item1();
        public static readonly Func<Numeral, Numeral> 
            DecreaseWithSwap = n =>
                ((Tuple<Numeral, Numeral>)n
                    (tuple => ((Tuple<Numeral, Numeral>)tuple).Shift(Increase))
                    (ChurchTuple<Numeral, Numeral>.Create(Zero)(Zero)))
                .Item1();
    }

    public static partial class NumeralExtensions
    {
        public static Numeral Increase(this Numeral n) => ChurchNumeral.Increase(n);

        public static Numeral Decrease(this Numeral n) => ChurchNumeral.Decrease(n);

        public static Numeral Add(this Numeral a, Numeral b) => ChurchNumeral.Add(a)(b);

        public static Numeral Subtract(this Numeral a, Numeral b) => ChurchNumeral.Subtract(a)(b);

        public static Numeral Multiply(this Numeral a, Numeral b) => ChurchNumeral.Multiply(a)(b);

        public static Numeral Pow(this Numeral mantissa, Numeral exponent) => ChurchNumeral.Pow(mantissa)(exponent);
    }

    public static partial class ChurchPredicate
    {
        public static readonly Func<Numeral, Boolean>
            IsZero = n => n(_ => False)(True);
    }

    public static partial class NumeralExtensions
    {
        public static Boolean IsZero(this Numeral n) => ChurchPredicate.IsZero(n);
    }

    public static partial class ChurchPredicate
    {
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LessOr")]
        public static readonly Func<Numeral, Func<Numeral, Boolean>>
            IsLessThanOrEqualTo = a => b => a.Subtract(b).IsZero();

        public static readonly Func<Numeral, Func<Numeral, Boolean>>
            IsGreaterThanOrEqualTo = a => b => b.Subtract(a).IsZero();

        public static readonly Func<Numeral, Func<Numeral, Boolean>>
            IsEqualTo = a => b => IsLessThanOrEqualTo(a)(b).And(IsGreaterThanOrEqualTo(a)(b));

        public static readonly Func<Numeral, Func<Numeral, Boolean>>
            IsGreaterThan = a => b => IsLessThanOrEqualTo(a)(b).Not();

        public static readonly Func<Numeral, Func<Numeral, Boolean>>
            IsLessThan = a => b => IsGreaterThanOrEqualTo(a)(b).Not();

        public static readonly Func<Numeral, Func<Numeral, Boolean>>
            IsNotEqualTo = a => b => IsEqualTo(a)(b).Not();
    }

    public static partial class NumeralExtensions
    {
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LessOr")]
        public static Boolean IsLessThanOrEqualTo(this Numeral a, Numeral b) => ChurchPredicate.IsLessThanOrEqualTo(a)(b);

        public static Boolean IsGreaterThanOrEqualTo(this Numeral a, Numeral b) => ChurchPredicate.IsGreaterThanOrEqualTo(a)(b);

        public static Boolean IsEqualTo(this Numeral a, Numeral b) => ChurchPredicate.IsEqualTo(a)(b);

        public static Boolean IsGreaterThan(this Numeral a, Numeral b) => ChurchPredicate.IsGreaterThan(a)(b);

        public static Boolean IsLessThan(this Numeral a, Numeral b) => ChurchPredicate.IsLessThan(a)(b);

        public static Boolean IsNotEqualTo(this Numeral a, Numeral b) => ChurchPredicate.IsNotEqualTo(a)(b);

        public static Numeral DivideBySelfReference(this Numeral dividend, Numeral divisor) => ChurchNumeral.DivideBySelfReference(dividend)(divisor);
    }
}
