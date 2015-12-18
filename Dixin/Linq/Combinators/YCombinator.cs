namespace Dixin.Linq.Combinators
{
    using System;

    public delegate Func<T, TResult> Recursion<T, TResult>(Recursion<T, TResult> f);

    public static class YCombinator
    {
        // Y = λf.(λx.f(x x)) (λx.f(x x))
        // Y = f => (λx.f(x x)) (λx.f(x x))
        // Y = f => (x => f(x(x)))(x => f(x(x)))
        // Y = (x => arg => f(x(x))(arg))(x => arg => f(x(x))(arg))
        public static Func<T, TResult> Y<T, TResult>
            (Func<Func<T, TResult>, Func<T, TResult>> f) =>
                new Recursion<T, TResult>(x => arg => f(x(x))(arg))(x => arg => f(x(x))(arg));
    }
}

namespace Dixin.Linq.Lambda
{
    using System;

    using Dixin.Linq.Combinators;

    public static partial class _NumeralExtensions
    {
        // Factorial = factorial => numeral => If(numeral.IsZero())(_ => One)(_ => factorial(numeral.Decrease()));
        public static Func<_Numeral, _Numeral> Factorial
            (Func<_Numeral, _Numeral> factorial) => numeral =>
                ChurchBoolean.If<_Numeral>(numeral.IsZero())
                    (_ => One)
                    (_ => factorial(numeral.Decrease()));

        public static _Numeral Factorial
            (this _Numeral numeral) => YCombinator.Y<_Numeral, _Numeral>(Factorial)(numeral);
    }

    public static partial class _NumeralExtensions
    {
        // Fibonacci  = fibonacci  => numeral => If(numeral > One)(_ => fibonacci(numeral - One) + fibonacci(numeral - One - One))(_ => numeral);
        public static Func<_Numeral, _Numeral> Fibonacci
            (Func<_Numeral, _Numeral> fibonacci) => numeral =>
                ChurchBoolean.If<_Numeral>(numeral > One)
                    (_ => fibonacci(numeral - One) + fibonacci(numeral - One - One))
                    (_ => numeral);

        public static _Numeral Fibonacci
            (this _Numeral numeral) => YCombinator.Y<_Numeral, _Numeral>(Fibonacci)(numeral);
    }

    public static partial class _NumeralExtensions
    {
        // DivideBy = divideBy => dividend => divisor => If(dividend >= divisor)(_ => One + divideBy(dividend - divisor)(divisor))(_ => Zero)
        public static Func<_Numeral, Func<_Numeral, _Numeral>> DivideBy
            (Func<_Numeral, Func<_Numeral, _Numeral>> divideBy) => dividend => divisor =>
                ChurchBoolean.If<_Numeral>(dividend >= divisor)
                    (_ => One + divideBy(dividend - divisor)(divisor))
                    (_ => Zero);

        public static _Numeral DivideBy
            (this _Numeral dividend, _Numeral divisor) =>
                YCombinator.Y<_Numeral, Func<_Numeral, _Numeral>>(DivideBy)(dividend)(divisor);
    }
}
