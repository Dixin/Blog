namespace Tutorial.LambdaCalculus
{
    using System;
    using System.Linq.Expressions;

    using static SkiCombinators;

#if DEMO
    public delegate T I<T>(T x);

    public delegate Func<T2, T1> K<T1, in T2>(T1 x);

    public delegate Func<Func<T1, T2>, Func<T1, TResult>> S<T1, T2, TResult>(Func<T1, Func<T2, TResult>> x);
#endif

#if DEMO
    // S = x => y => z = x(z)(y(z))
    public static Func<Func<T1, T2>, Func<T1, TResult>> S<T1, T2, TResult>
        (Func<T1, Func<T2, TResult>> x) => y => z => x(z)(y(z));

    // K = x => _ => x
    public static Func<T2, T1> K<T1, T2>
        (T1 x) => _ => x;

    // I = x => x
    public static T I<T>
        (T x) => x;

    // True = K
    public static Func<object, object> True
        (object @true) => K<object, object>(@true);

    // False = S(K)
    public static Func<object, object> False
        (object @true) => @false =>
            S<object, object, object>(K<object, object>)((Func<object, object>)@true)(@false);
#endif

    public static partial class SkiCombinators
    {
        public static readonly Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>
            S = x => y => z => x(z)(y(z));

        public static readonly Func<dynamic, Func<dynamic, dynamic>>
            K = x => y => x;

        public static readonly Func<dynamic, dynamic>
            I = x => x;
    }

    public static partial class SkiCalculus
    {
        public static readonly Boolean
            True = new Boolean(K);

        public static readonly Boolean
            False = new Boolean(S(K));

        public static readonly Func<dynamic, dynamic>
            Compose = S(K(S))(K);

        public static readonly Func<dynamic, dynamic>
            Zero = K(I);

        public static readonly Func<dynamic, dynamic>
            One = I;

        public static readonly Func<dynamic, dynamic>
            Two = S(Compose)(I);

        public static readonly Func<dynamic, dynamic>
            Three = S(Compose)(S(Compose)(I));

        // ...

        public static readonly Func<dynamic, Func<dynamic, dynamic>>
            Increase = S(Compose);

        public static readonly Func<dynamic, dynamic>
            ω = S(I)(I);

#if DEMO
        public static readonly Func<dynamic, dynamic>
            Ω = S(I)(I)(S(I)(I));
#endif

        public static readonly Func<dynamic, dynamic>
            IWithSK = S(K)(K); // Or S(K)(S).
    }

    public static partial class SkiCalculus
    {
        internal static void FunctionAsData<T>()
        {
            Func<T, T> idFunction = value => value;
            Expression<Func<T, T>> idExpression = value => value;
        }

        public static readonly Func<dynamic, bool>
           UnchurchBoolean = boolean => boolean(true)(false);

        public static readonly Func<dynamic, uint>
           UnchurchNumeral = n => n(new Func<uint, uint>(x => x + 1))(0U);
    }
}
