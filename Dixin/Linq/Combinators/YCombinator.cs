namespace Dixin.Linq.Combinators
{
    using System;

    public delegate Func<T, TResult> Recursion<T, TResult>(Recursion<T, TResult> f);

    public static class YCombinator<T, TResult>
    {
        // Y = λf.(λx.f(x x)) (λx.f(x x))
        // Y = f => (λx.f(x x)) (λx.f(x x))
        // Y = f => (x => f(x(x)))(x => f(x(x)))
        // Y = (x => arg => f(x(x))(arg))(x => arg => f(x(x))(arg))
        public static readonly Func<Func<Func<T, TResult>, Func<T, TResult>>, Func<T, TResult>>
            Y = f => new Recursion<T, TResult>(x => arg => f(x(x))(arg))(x => arg => f(x(x))(arg));
    }
}

namespace Dixin.Linq.Lambda
{
    using System;

    using Dixin.Linq.Combinators;

    using static NumeralWrapper;

    public static partial class ChurchNumeral
    {
        // Factorial = factorial => numeral => If(numeral.IsZero())(_ => One)(_ => factorial(numeral.Decrease()));
        public static Func<NumeralWrapper, NumeralWrapper> Factorial(Func<NumeralWrapper, NumeralWrapper> factorial) => numeral =>
            ChurchBoolean<NumeralWrapper>.If(numeral.IsZero())
                (_ => NumeralWrapper.One)
                (_ => factorial(numeral.Decrease()));

        public static NumeralWrapper Factorial(this NumeralWrapper numeral) => YCombinator<NumeralWrapper, NumeralWrapper>.Y(Factorial)(numeral);
    }

    public static partial class ChurchNumeral
    {
        // Fibonacci  = fibonacci  => numeral => If(numeral > One)(_ => fibonacci(numeral - One) + fibonacci(numeral - One - One))(_ => numeral);
        private static Func<NumeralWrapper, NumeralWrapper> Fibonacci(Func<NumeralWrapper, NumeralWrapper> fibonacci) => numeral =>
            ChurchBoolean<NumeralWrapper>.If(numeral > NumeralWrapper.One)
                (_ => fibonacci(numeral - NumeralWrapper.One) + fibonacci(numeral - NumeralWrapper.One - NumeralWrapper.One))
                (_ => numeral);

        public static NumeralWrapper Fibonacci(this NumeralWrapper numeral) => YCombinator<NumeralWrapper, NumeralWrapper>.Y(Fibonacci)(numeral);
    }

    public static partial class ChurchNumeral
    {
        // DivideBy = divideBy => dividend => divisor => If(dividend >= divisor)(_ => One + divideBy(dividend - divisor)(divisor))(_ => Zero)
        private static Func<NumeralWrapper, Func<NumeralWrapper, NumeralWrapper>> DivideBy(
            Func<NumeralWrapper, Func<NumeralWrapper, NumeralWrapper>> divideBy) => dividend => divisor =>
                ChurchBoolean<NumeralWrapper>.If(dividend >= divisor)
                    (_ => NumeralWrapper.One + divideBy(dividend - divisor)(divisor))
                    (_ => NumeralWrapper.Zero);

        public static NumeralWrapper DivideBy(this NumeralWrapper dividend, NumeralWrapper divisor) =>
            YCombinator<NumeralWrapper, Func<NumeralWrapper, NumeralWrapper>>.Y(DivideBy)(dividend)(divisor);
    }

    public static partial class ChurchNumeral
    {
        // Factorial = factorial => numeral => If(numeral.IsZero())(_ => One)(_ => factorial(numeral.Decrease()));
        public static Func<Numeral, Numeral> Factorial(Func<Numeral, Numeral> factorial) => numeral =>
            ChurchBoolean<Numeral>.If(numeral.IsZero())
                (_ => One)
                (_ => factorial(numeral.Decrease()));

        public static Numeral Factorial(this Numeral numeral) => YCombinator<Numeral, Numeral>.Y(Factorial)(numeral);
    }

    public static partial class ChurchNumeral
    {
        // Fibonacci  = fibonacci  => numeral => If(numeral > One)(_ => fibonacci(numeral - One) + fibonacci(numeral - One - One))(_ => numeral);
        private static Func<Numeral, Numeral> Fibonacci(Func<Numeral, Numeral> fibonacci) => numeral =>
            ChurchBoolean<Numeral>.If(numeral.Greater(One))
                (_ => fibonacci(numeral.Subtract(One)).Add(fibonacci(numeral.Subtract(One).Subtract(One))))
                (_ => numeral);

        public static Numeral Fibonacci(this Numeral numeral) => YCombinator<Numeral, Numeral>.Y(Fibonacci)(numeral);
    }

    public static partial class ChurchNumeral
    {
        // DivideBy = divideBy => dividend => divisor => If(dividend >= divisor)(_ => One + divideBy(dividend - divisor)(divisor))(_ => Zero)
        private static Func<Numeral, Func<Numeral, Numeral>> DivideBy(
            Func<Numeral, Func<Numeral, Numeral>> divideBy) => dividend => divisor =>
                ChurchBoolean<Numeral>.If(dividend.GreaterOrEqual(divisor))
                    (_ => One.Add(divideBy(dividend.Subtract(divisor))(divisor)))
                    (_ => Zero);

        public static Numeral DivideBy(this Numeral dividend, Numeral divisor) =>
            YCombinator<Numeral, Func<Numeral, Numeral>>.Y(DivideBy)(dividend)(divisor);
    }
}
