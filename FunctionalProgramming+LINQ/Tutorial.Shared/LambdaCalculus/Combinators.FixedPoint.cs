namespace Tutorial.LambdaCalculus
{
    using System;

    using static ChurchBoolean;

    using static FixedPointCombinators<Numeral, Numeral>;

    public static partial class FixedPointCombinators<T, TResult>
    {
        // Y = (g => f(g(g)))(g => f(g(g)))
        public static readonly Func<Func<Func<T, TResult>, Func<T, TResult>>, Func<T, TResult>>
            Y = f => new SelfApplicableFunc<Func<T, TResult>>(g => f(g(g)))(g => f(g(g)));
    }

    public static partial class FixedPointCombinators<T, TResult>
    {
        // Z = (g => x => f(g(g))(x))(g => x => f(g(g))(x))
        public static readonly Func<Func<Func<T, TResult>, Func<T, TResult>>, Func<T, TResult>>
            Z = f => new SelfApplicableFunc<Func<T, TResult>>(g => x => f(g(g))(x))(g => x => f(g(g))(x));
    }

    public static partial class ChurchNumeral
    {
        // FactorialHelper = factorial => n => If(n == 0)(_ => 1)(_ => n * factorial(n - 1))
        public static readonly Func<Func<Numeral, Numeral>, Func<Numeral, Numeral>>
            FactorialHelper = factorial => n =>
                If(n.IsZero())
                    (_ => One)
                    (_ => n.Multiply(factorial(n.Subtract(One))));
#if DEMO
        public static readonly Func<Numeral, Numeral>
            Factorial = Y(FactorialHelper);
#endif
    }

    public static partial class ChurchNumeral
    {
        public static readonly Func<Numeral, Numeral>
            Factorial = Z(FactorialHelper);
    }

    public static partial class ChurchNumeral
    {
        // FibonacciHelper = fibonacci => n => If(n > 1)(_ => fibonacci(n - 1) + fibonacci(n - 2))(_ => n)
        private static readonly Func<Func<Numeral, Numeral>, Func<Numeral, Numeral>>
            FibonacciHelper = fibonacci => n =>
                If(n.IsGreaterThan(One))
                    (_ => fibonacci(n.Subtract(One)).Add(fibonacci(n.Subtract(Two))))
                    (_ => n);

        public static readonly Func<Numeral, Numeral>
            Fibonacci = Z(FibonacciHelper);
    }
}

namespace Tutorial.LambdaCalculus
{
    using System;

    using static ChurchBoolean;
    using static FixedPointCombinators<Numeral, System.Func<Numeral, Numeral>>;

    public static partial class ChurchNumeral
    {
        // DivideByHelper = divideBy => dividend => divisor => If(dividend >= divisor)(_ => 1 + divideBy(dividend - divisor)(divisor))(_ => 0)
        private static readonly Func<Func<Numeral, Func<Numeral, Numeral>>, Func<Numeral, Func<Numeral, Numeral>>> DivideByHelper = divideBy => dividend => divisor =>
                If(dividend.IsGreaterThanOrEqualTo(divisor))
                    (_ => One.Add(divideBy(dividend.Subtract(divisor))(divisor)))
                    (_ => Zero);

        public static readonly Func<Numeral, Func<Numeral, Numeral>> 
            DivideBy = Z(DivideByHelper);
    }

    public static partial class NumeralExtensions
    {
        public static Numeral Factorial(this Numeral n) => ChurchNumeral.Factorial(n);

        public static Numeral Fibonacci(this Numeral n) => ChurchNumeral.Fibonacci(n);

        public static Numeral DivideBy(this Numeral dividend, Numeral divisor) => ChurchNumeral.DivideBy(dividend)(divisor);
    }
}
