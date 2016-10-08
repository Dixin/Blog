namespace Dixin.Linq.Lambda
{
    using System;

    public delegate Func<T, T> Boolean<T>(T @true);

    public static partial class ChurchBoolean<T>
    {
        // True = @true => @false => @true
        public static readonly Boolean<T>
            True = @true => @false => @true;

        // False = @true => @false => @false
        public static readonly Boolean<T>
            False = @true => @false => @false;
    }

    public static partial class ChurchBoolean<T>
    {
        // And = a => b => a(b)(False)
        public static Func<Boolean<Boolean<T>, Boolean<T>>, Func<Boolean<T>, Boolean<T>>>
            And = a => b => (Boolean<T>)a(b)(False);

        // Or = a => b => a(True)(b)
        public static Func<Boolean<Boolean<T>, Boolean<T>>, Func<Boolean<T>, Boolean<T>>>
            Or = a => b => (Boolean<T>)a(True)(b);

        // Not = boolean => boolean(False)(True)
        public static Func<Boolean<Boolean<T>, Boolean<T>>, Boolean<T>>
            Not = boolean => (Boolean<T>)boolean(False)(True);

        // Xor = a => b => a(b(False)(True))(b(True)(False))
        public static Func<Boolean<Boolean<T>, Boolean<T>>, Func<Boolean<Boolean<T>, Boolean<T>>, Boolean<T>>>
            Xor = a => b => (Boolean<T>)a((Boolean<T>)b(False)(True))((Boolean<T>)b(True)(False));
    }

    public static partial class ChurchBoolean
    {
        // And = a => b => a(b)(False)
        public static Boolean<T> And<T>(this Boolean<Boolean<T>, Boolean<T>> a, Boolean<T> b) =>
                (Boolean<T>)a(b)(ChurchBoolean<T>.False);

        // Or = a => b => a(True)(b)
        public static Boolean<T> Or<T>(this Boolean<Boolean<T>, Boolean<T>> a, Boolean<T> b) =>
                (Boolean<T>)a(ChurchBoolean<T>.True)(b);

        // Not = boolean => boolean(False)(True)
        public static Boolean<T> Not<T>(this Boolean<Boolean<T>, Boolean<T>> boolean) =>
                (Boolean<T>)boolean(ChurchBoolean<T>.False)(ChurchBoolean<T>.True);

        // Xor = a => b => a(b(False)(True))(b(True)(False))
        public static Boolean<T> Xor<T>(this Boolean<Boolean<T>, Boolean<T>> a, Boolean<Boolean<T>, Boolean<T>> b) =>
                (Boolean<T>)a((Boolean<T>)b(ChurchBoolean<T>.False)(ChurchBoolean<T>.True))((Boolean<T>)b(ChurchBoolean<T>.True)(ChurchBoolean<T>.False));
    }

    // Curried from: object Boolean(TTrue @true, TFalse @TFalse)
    // Boolean is alias of Func<TTrue, Func<TFalse, object>>
    public delegate Func<TFalse, object> Boolean<in TTrue, in TFalse>(TTrue @true);

    public static partial class ChurchBoolean<TTrue, TFalse>
    {
        // True = @true => @false => @true
        public static readonly Boolean<TTrue, TFalse>
            True = @true => @false => @true;

        // False = @true => @false => @false
        public static readonly Boolean<TTrue, TFalse>
            False = @true => @false => @false;
    }

    public static partial class ChurchBoolean
    {
        // And = a => b => a(b)(False)
        public static Boolean<TTrue, TFalse> And<TTrue, TFalse>(
            this Boolean<Boolean<TTrue, TFalse>, Boolean<TTrue, TFalse>> a, Boolean<TTrue, TFalse> b) =>
                (Boolean<TTrue, TFalse>)a(b)(ChurchBoolean<TTrue, TFalse>.False);

        // Or = a => b => a(True)(b)
        public static Boolean<TTrue, TFalse> Or<TTrue, TFalse>(
            this Boolean<Boolean<TTrue, TFalse>, Boolean<TTrue, TFalse>> a, Boolean<TTrue, TFalse> b) =>
                (Boolean<TTrue, TFalse>)a(ChurchBoolean<TTrue, TFalse>.True)(b);

        // Not = boolean => boolean(False)(True)
        public static Boolean<TTrue, TFalse> Not<TTrue, TFalse>(
            this Boolean<Boolean<TTrue, TFalse>, Boolean<TTrue, TFalse>> boolean) =>
                (Boolean<TTrue, TFalse>)boolean(ChurchBoolean<TTrue, TFalse>.False)(ChurchBoolean<TTrue, TFalse>.True);

        // Xor = a => b => a(b(False)(True))(b(True)(False))
        public static Boolean<TTrue, TFalse> Xor<TTrue, TFalse>(
            this Boolean<Boolean<TTrue, TFalse>, Boolean<TTrue, TFalse>> a, Boolean<Boolean<TTrue, TFalse>, Boolean<TTrue, TFalse>> b) =>
                (Boolean<TTrue, TFalse>)a((Boolean<TTrue, TFalse>)b(ChurchBoolean<TTrue, TFalse>.False)(ChurchBoolean<TTrue, TFalse>.True))((Boolean<TTrue, TFalse>)b(ChurchBoolean<TTrue, TFalse>.True)(ChurchBoolean<TTrue, TFalse>.False));
    }
}
