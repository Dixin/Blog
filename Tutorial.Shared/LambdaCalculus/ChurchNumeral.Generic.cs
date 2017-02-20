namespace Tutorial.LambdaCalculus
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using static NumeralWrapper;
    using static ChurchBoolean;

    // Curried from T Numeral<T>(Func<T, T> f, T x).
    // Numeral<T> is the alias for FuncFunc<T, T>, Func<T, T>>.
    public delegate Func<T, T> Numeral<T>(Func<T, T> f);

    public partial class NumeralWrapper
    {
        private readonly NumeralWrapper predecessor;

        public NumeralWrapper(NumeralWrapper predecessor)
        {
            this.predecessor = predecessor;
        }

        protected virtual NumeralWrapper Predecessor => this.predecessor;

        public virtual Func<T, T> Invoke<T>(Func<T, T> f) =>
            f.o(this.Predecessor.Invoke(f));
    }

    public partial class NumeralWrapper
    {
        private NumeralWrapper()
        {
        }

        private class ZeroNumeralWrapper : NumeralWrapper
        {
            protected override NumeralWrapper Predecessor => this;

            public override Func<T, T> Invoke<T>(Func<T, T> f) =>
                x => x;
        }

        public static NumeralWrapper Zero { get; } = new ZeroNumeralWrapper();

        public static NumeralWrapper One { get; } = Zero.Increase();
    }

    public static partial class NumeralWrapperExtensions
    {
        // Increase = n => new Numeral(n)
        public static NumeralWrapper Increase(this NumeralWrapper numeral) => new NumeralWrapper(numeral);

        // Add = a => b => a(Increase)(b)
        public static NumeralWrapper Add(this NumeralWrapper a, NumeralWrapper b) => a.Invoke<NumeralWrapper>(Increase)(b);
    }

    public static partial class NumeralWrapperExtensions
    {
        // ...

        // Decrease = n => f => x => n(g => h => h(g(f)))(_ => x)(_ => _)
        public static NumeralWrapper Decrease(this NumeralWrapper numeral) =>
            new Numeral<NumeralWrapper>(f => x =>
                numeral.Invoke<Func<Func<NumeralWrapper, NumeralWrapper>, NumeralWrapper>>(g => h => h(g(f)))(_ => x)(_ => _))
                (Increase)(Zero);

        // Subtract = a => b => b(Decrease)(a)
        public static NumeralWrapper Subtract(this NumeralWrapper a, NumeralWrapper b) => b.Invoke<NumeralWrapper>(Decrease)(a);
    }

    public static partial class NumeralWrapperExtensions
    {
        // Multiply = a => b => a(x => b.Add(x))(Zero)
        public static NumeralWrapper Multiply(this NumeralWrapper a, NumeralWrapper b) => a.Invoke<NumeralWrapper>(b.Add)(Zero);

        // Power = m => e => e(x => m.Multiply(x))(1)
        public static NumeralWrapper Pow(this NumeralWrapper mantissa, NumeralWrapper exponent) =>
            exponent.Invoke<NumeralWrapper>(mantissa.Multiply)(One);

        // _DivideBy = dividend => divisor => 
        // If(dividend.IsGreaterOrEqual(divisor))
        //    (_ => One + (dividend - divisor)._DivideBy(divisor))
        //    (_ => Zero);
        public static NumeralWrapper _DivideBy(this NumeralWrapper dividend, NumeralWrapper divisor) =>
            If(dividend >= divisor)
                (_ => One + (dividend - divisor)._DivideBy(divisor))
                (_ => Zero);
    }

    public partial class NumeralWrapper
    {
        public static NumeralWrapper operator +(NumeralWrapper a, NumeralWrapper b) => a.Add(b);

        public static NumeralWrapper operator -(NumeralWrapper a, NumeralWrapper b) => a.Subtract(b);

        public static NumeralWrapper operator *(NumeralWrapper a, NumeralWrapper b) => a.Multiply(b);

        public static NumeralWrapper operator ^(NumeralWrapper a, NumeralWrapper b) => a.Pow(b);

        public static NumeralWrapper operator ++(NumeralWrapper numeral) => numeral.Increase();

        public static NumeralWrapper operator --(NumeralWrapper numeral) => numeral.Decrease();
    }

    public partial class NumeralWrapper
    {
        public static NumeralWrapper operator /(NumeralWrapper a, NumeralWrapper b) => a._DivideBy(b);
    }

    public partial class NumeralWrapper
    {
        public static bool operator ==(NumeralWrapper a, uint b) => a.Unchurch() == b;

        public static bool operator ==(uint a, NumeralWrapper b) => a == b.Unchurch();

        public static bool operator !=(NumeralWrapper a, uint b) => a.Unchurch() != b;

        public static bool operator !=(uint a, NumeralWrapper b) => a != b.Unchurch();
    }

    public partial class NumeralWrapper
    {
        public static Boolean operator <=(NumeralWrapper a, NumeralWrapper b) => a.LessOrEqual(b);

        public static Boolean operator >=(NumeralWrapper a, NumeralWrapper b) => a.GreaterOrEqual(b);

        public static Boolean operator <(NumeralWrapper a, NumeralWrapper b) => a.Less(b);

        public static Boolean operator >(NumeralWrapper a, NumeralWrapper b) => a.Greater(b);

        public static Boolean operator ==(NumeralWrapper a, NumeralWrapper b) => a.Equal(b);

        public static Boolean operator !=(NumeralWrapper a, NumeralWrapper b) => a.NotEqual(b);
    }

    public partial class NumeralWrapper
    {
        public override int GetHashCode() => this.Unchurch().GetHashCode();

        public override bool Equals(object obj) => 
            obj is NumeralWrapper numeral && (object)numeral != null && this.Equal(numeral).Unchurch();
    }

    public static partial class NumeralWrapperExtensions
    {
        // IsZero = n => n(_ => False)(True)
        public static Boolean IsZero(this NumeralWrapper numeral) =>
            numeral.Invoke<Boolean>(_ => False)(True);
    }

    public static partial class NumeralWrapperExtensions
    {
        // IsLessOrEqual = a => b => a.Subtract(b).IsZero()
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LessOr")]
        public static Boolean LessOrEqual(this NumeralWrapper a, NumeralWrapper b) => a.Subtract(b).IsZero();

        // IsGreaterOrEqual = a => b => b.Subtract(a).IsZero()
        public static Boolean GreaterOrEqual(this NumeralWrapper a, NumeralWrapper b) => b.Subtract(a).IsZero();

        // IsLess = a => b => a.IsGreaterOrEqual(b).Not()
        public static Boolean Less(this NumeralWrapper a, NumeralWrapper b) => a.GreaterOrEqual(b).Not();

        // IsGreater = a => b => a.IsLessOrEqual(b).Not()
        public static Boolean Greater(this NumeralWrapper a, NumeralWrapper b) => a.LessOrEqual(b).Not();

        // AreEqual = a => b => a.Subtract(b).IsZero().And(a.Subtract(b).IsZero())
        // Or:
        // AreEqual = a => b => a.IsLessOrEqual(b).And(a.IsGreaterOrEqual(b))
        public static Boolean Equal(this NumeralWrapper a, NumeralWrapper b) => a.LessOrEqual(b).And(a.GreaterOrEqual(b));

        // AreNotEqual = a => b => a.AreEqual(b).Not()
        public static Boolean NotEqual(this NumeralWrapper a, NumeralWrapper b) => a.Equal(b).Not();
    }

    public partial class NumeralWrapper
    {
        public static NumeralWrapper operator |(NumeralWrapper dividend, NumeralWrapper divisor) => dividend.DivideByIgnoreZero(divisor);
    }

    public static partial class NumeralWrapperExtensions
    {
        // DivideByIgnoreZero = dividend => divisor => If(divisor.IsZero())(_ => Zero)(_ => dividend._DivideBy(divisor))
        public static NumeralWrapper DivideByIgnoreZero(this NumeralWrapper dividend, NumeralWrapper divisor) =>
            If(divisor.IsZero())
                (_ => Zero)
                (_ => dividend._DivideBy(divisor));

        // Decrease2 = n => n(tuple => tuple.Shift(Increase))(ChurchTuple.Create(Zero)(Zero)).Item1();
        public static NumeralWrapper Decrease2(this NumeralWrapper numeral) =>
            numeral.Invoke<Tuple<NumeralWrapper, NumeralWrapper>>
                (tuple => tuple.Shift(Increase)) // (x, y) -> (y, y + 1)
                (ChurchTuple<NumeralWrapper, NumeralWrapper>.Create(Zero)(Zero))
            .Item1();
    }

    public static partial class ChurchEncoding
    {
        public static NumeralWrapper ChurchWarpper(this uint n) => n > 0U ? new NumeralWrapper(ChurchWarpper(n - 1U)) : Zero;

        public static uint Unchurch(this NumeralWrapper numeral) => numeral.Invoke<uint>(x => x + 1)(0U);

        public static string Visualize(this NumeralWrapper numeral) =>
            numeral.Invoke<string>(x => string.Concat(x, "*"))(string.Empty);
    }

    public static partial class ChurchNumeralWrapper
    {
        // Factorial = factorial => numeral => If(numeral.IsZero())(_ => 1)(_ => factorial(numeral.Decrease()) * numeral);
        public static Func<NumeralWrapper, NumeralWrapper> Factorial(Func<NumeralWrapper, NumeralWrapper> factorial) => numeral =>
            If(numeral.IsZero())
                (_ => One)
                (_ => factorial(numeral - One) * numeral);

        public static NumeralWrapper Factorial(this NumeralWrapper numeral) => FixedPointCombinators<NumeralWrapper, NumeralWrapper>.Z(Factorial)(numeral);

        // Fibonacci  = fibonacci  => numeral => If(numeral > 1)(_ => fibonacci(numeral - One) + fibonacci(numeral - 1 - 1))(_ => numeral);
        private static Func<NumeralWrapper, NumeralWrapper> Fibonacci(Func<NumeralWrapper, NumeralWrapper> fibonacci) => numeral =>
            If(numeral > One)
                (_ => fibonacci(numeral - One) + fibonacci(numeral - One - One))
                (_ => numeral);

        public static NumeralWrapper Fibonacci(this NumeralWrapper numeral) => FixedPointCombinators<NumeralWrapper, NumeralWrapper>.Z(Fibonacci)(numeral);

        // DivideBy = divideBy => dividend => divisor => If(dividend >= divisor)(_ => One + divideBy(dividend - divisor)(divisor))(_ => Zero)
        private static Func<NumeralWrapper, Func<NumeralWrapper, NumeralWrapper>> DivideBy(
            Func<NumeralWrapper, Func<NumeralWrapper, NumeralWrapper>> divideBy) => dividend => divisor =>
                If(dividend >= divisor)
                    (_ => One + divideBy(dividend - divisor)(divisor))
                    (_ => Zero);

        public static NumeralWrapper DivideBy(this NumeralWrapper dividend, NumeralWrapper divisor) =>
            FixedPointCombinators<NumeralWrapper, Func<NumeralWrapper, NumeralWrapper>>.Z(DivideBy)(dividend)(divisor);
    }
}
