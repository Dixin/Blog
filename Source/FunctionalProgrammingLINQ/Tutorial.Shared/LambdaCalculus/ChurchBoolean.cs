namespace Tutorial.LambdaCalculus
{
    using System;

#if DEMO
    using static Tutorial.Lambda.Boolean;
#endif

    // Curried from (dynamic, dynamic) -> dynamic.
    // Boolean is the alias of dynamic -> dynamic -> dynamic,
    public delegate Func<dynamic, dynamic> Boolean(dynamic @true);

    public static partial class ChurchBoolean
    {
        public static readonly Boolean
            True = @true => @false => @true;

        public static readonly Boolean
            False = @true => @false => @false;
    }

    public static partial class ChurchBoolean
    {
        public static readonly Func<Boolean, Func<Boolean, Boolean>>
            And = a => b => a(b)(False);
    }

#if DEMO
    public delegate Func<object, object> Boolean(object @true);

    public static partial class ChurchBoolean
    {
        public static readonly Func<Boolean, Func<Boolean, Boolean>>
            And2 = a => b => (Boolean)a(b)(False);
    }
#endif

    public static partial class ChurchBoolean
    {
#if DEMO
        // And = a => b => a(b)(true => false => false)
        public static readonly Func<Boolean, Func<Boolean, Boolean>>
            And = a => b => a(b)(new Boolean(@true => @false => @false));
#endif

        public static readonly Func<Boolean, Func<Boolean, Boolean>> 
            Or = a => b => a(True)(b);

        public static readonly Func<Boolean, Boolean> 
            Not = boolean => boolean(False)(True);

        public static readonly Func<Boolean, Func<Boolean, Boolean>>
            Xor = a => b => a(Not(b))(b);
    }

    public static partial class BooleanExtensions
    {
        public static Boolean And(this Boolean a, Boolean b) => ChurchBoolean.And(a)(b);
    }

    public static partial class ChurchBoolean
    {
        internal static void CallAnd()
        {
            Boolean result1 = True.And(True);

            Boolean x = True;
            Boolean y = False;
            Boolean result2 = x.And(y);
        }

        internal static void CallAnonymousAnd()
        {
            Boolean result1 = new Func<Boolean, Func<Boolean, Boolean>>(a => b => (Boolean)a(b)(False))(True)(True);

            Boolean x = True;
            Boolean y = False;
            Boolean result2 = new Func<Boolean, Func<Boolean, Boolean>>(a => b => (Boolean)a(b)(False))(x)(y);
        }
    }

    public static partial class BooleanExtensions
    {
        public static Boolean Or(this Boolean a, Boolean b) => ChurchBoolean.Or(a)(b);

        public static Boolean Not(this Boolean a) => ChurchBoolean.Not(a);

        public static Boolean Xor(this Boolean a, Boolean b) => ChurchBoolean.Xor(a)(b);
    }

    public static partial class ChurchBoolean
    {
        // EagerIf = condition => then => @else => condition(then)(@else)
        public static readonly Func<Boolean, Func<dynamic, Func<dynamic, dynamic>>>
            EagerIf = condition => then => @else =>
                condition    // if (condition)
                    (then)   // then { ... }
                    (@else); // else { ... }

        // If = condition => thenFactory => elseFactory => condition(thenFactory, elseFactory)(Id)
        public static readonly Func<Boolean, Func<Func<Unit<dynamic>, dynamic>, Func<Func<Unit<dynamic>, dynamic>, dynamic>>>
            If = condition => thenFactory => elseFactory =>
                condition
                    (thenFactory)
                    (elseFactory)(Functions<dynamic>.Id);
    }

    public static partial class ChurchBoolean
    {
        internal static void CallEagerIf(Boolean condition, Boolean a, Boolean b)
        {
            Boolean result = EagerIf(condition)
                (a.And(b)) // then.
                (a.Or(b)); // else.
        }

        internal static void CallLazyIf(Boolean condition, Boolean a, Boolean b)
        {
            Boolean result = If(condition)
                (_ => a.And(b)) // then.
                (_ => a.Or(b)); // else.
        }
    }
}