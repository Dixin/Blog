namespace Dixin.Linq.Combinators.Obsolete
{
    using System;

    public static partial class SkiCombinators
    {
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

        // Cannot be compiled.
        // False = S(K)
        //public static Func<object, object> False
        //    (object /* Func<object, object> */ @true) => @false => 
        //        S<object, object, object>(K<object, object>)(/* Func<object, object> */ @true)(@false);

        // False = S(K)
        public static Func<object, object> False
            (dynamic @true) => @false => S<object, object, object>(K<object, object>)(@true)(@false);
    }
}

namespace Dixin.Linq.Combinators
{
    using System;

    public delegate T I<T>(T x);

    public delegate Func<T2, T1> K<T1, in T2>(T1 x);

    public delegate Func<Func<T1, T2>, Func<T1, TResult>> S<T1, T2, TResult>(Func<T1, Func<T2, TResult>> x);



    public static partial class SkiCombinators
    {
        // S = x => y => z = x(z)(y(z))
        public static Func<Func<T1, T2>, Func<T1, TResult>> _S<T1, T2, TResult>
            (Func<T1, Func<T2, TResult>> x) => y => z => x(z)(y(z));

        // K = x => _ => x
        public static Func<T2, T1> _K<T1, T2>
            (T1 x) => BckwCombinators.K<T1, T2>(x);

        // I = x => x
        public static T _I<T>
            (T x) => x;
    }

    public delegate Func<dynamic, dynamic> Boolean(dynamic @true);

    public static partial class SkiCombinators
    {
        public static Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>
            S = x => y => z => x(z)(y(z));

        public static Func<dynamic, Func<dynamic, dynamic>>
            K = x => y => x;

        public static Func<dynamic, dynamic>
            I = x => x;

        public static Boolean
            True = new Boolean(K);

        public static Boolean
            False = new Boolean(S(K));

        public static Func<dynamic, dynamic>
            I2 = S(K)(K);

        public static Func<dynamic, dynamic>
            I3 = S(K)(S);

        public static Func<Boolean, Boolean>
            Not = boolean => boolean(S(K))(K);
        public static Func<Boolean, Func<Boolean, Boolean>>
            Or = a => b => a(K)(b);
        public static Func<Boolean, Func<Boolean, Boolean>>
            And = a => b => a(b)(S(K));
        public static Func<Boolean, Func<Boolean, Boolean>>
            Xor = a => b => a(b(S(K))(K))(b(K)(S(K)));

        public static Func<dynamic, dynamic>
            Compose = S(K(S))(K);

        public static Func<dynamic, dynamic>
            Zero = K(I);

        public static Func<dynamic, dynamic>
            One = I;

        public static Func<dynamic, dynamic>
            Two = S(Compose)(I);

        public static Func<dynamic, dynamic>
            Three = S(Compose)(S(Compose)(I));

        public static Func<dynamic, Func<dynamic, dynamic>>
            Increase = S(Compose);

        public static Func<dynamic, bool>
            _UnchurchBoolean = boolean => (bool)boolean(true)(false);

        public static Func<dynamic, uint>
            _UnchurchNumeral = numeral => numeral(new Func<uint, uint>(x => x + 1))(0U);

        public static Func<dynamic, dynamic>
            ω = S(I)(I);

        public static Func<dynamic, dynamic>
            Ω = _ => ω(ω); // Ω = ω(ω) throws exception.
    }
}
