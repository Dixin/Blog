namespace Dixin.Linq.Lambda
{
    using System;

    // Curried from: object Boolean(object @true, object @false)
    // Boolean is just alias for Func<object, Func<object, object>>
    public delegate Func<object, object> Boolean(object @true);

    public static partial class ChurchBoolean
    {
        public static Func<object, object> True
            (object @true) => @false => @true;

        public static Func<object, object> False
            (object @true) => @false => @false;

        // Not preferred:
        [Obsolete]
        public static Boolean False2 =
            @true => @false => @false;

        [Obsolete]
        public static Boolean True2 =
            @true => @false => @true;
    }

    public static partial class ChurchBoolean
    {
        // And = a => b => a(b)(False)
        public static Boolean And
            (this Boolean a, Boolean b) =>
                // (Boolean<TTrue, TFalse>)a(b)(False<TTrue, TFalse>);
                // The casting to Boolean is safe, because b and False are both of type Boolean.
                (Boolean)a(b)(new Boolean(False));
    }

    public static partial class ChurchBoolean
    {
        // Or = a => b => a(True)(b)
        public static Boolean Or
            (this Boolean a, Boolean b) => (Boolean)a(new Boolean(True))(b);

        // Not = boolean => boolean(False)(True)
        public static Boolean Not
            (this Boolean boolean) => (Boolean)boolean(new Boolean(False))(new Boolean(True));

        // Xor = a => b => a(b(False)(True))(b(True)(False))
        public static Boolean Xor
            (this Boolean a, Boolean b) =>
                (Boolean)a
                    (b(new Boolean(False))(new Boolean(True)))
                    (b(new Boolean(True))(new Boolean(False)));
    }
    
    public static partial class ChurchBoolean
    {
        // If1 = condition => then => @else => condition(then, @else)
        public static Func<T, Func<T, T>> If1<T>
            (Boolean condition) => then => @else =>
                (T)condition
                    (then)
                    (@else);
    }

    public static partial class ChurchBoolean
    {
        // If2 = condition => then => @else => condition(then, @else)()
        public static Func<Func<T>, Func<Func<T>, T>> If2<T>
            (Boolean condition) => then => @else =>
                ((Func<T>)condition
                    (then)
                    (@else))();
    }

    public static partial class ChurchBoolean
    {
        public static Func<Func<Func<T, T>, T>, Func<Func<Func<T, T>, T>, T>> If<T>
            (Boolean condition) => then => @else =>
                ((Func<Func<T, T>, T>)condition
                    (then)
                    (@else))(_ => _);
    }
}