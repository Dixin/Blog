namespace Dixin.Linq.Lambda
{
    using System;

    // Curried from: object Boolean(TTrue @true, TFalse @TFalse)
    public delegate Func<TFalse, object> Boolean<in TTrue, in TFalse>(TTrue @true);
    // Boolean is alias of Func<TTrue, Func<TFalse, object>>

    public static partial class ChurchBoolean
    {
        // True = @true => @false => @true
        public static Func<TFalse, object> True<TTrue, TFalse>
            (TTrue @true) => @false => @true;

        // False = @true => @false => @false
        public static Func<TFalse, object> False<TTrue, TFalse>
            (TTrue @true) => @false => @false;
    }

    public static partial class ChurchBoolean
    {
        // And = a => b => a(b)(False)
        public static Boolean<TTrue, TFalse> And<TTrue, TFalse>
            (this Boolean<Boolean<TTrue, TFalse>, Boolean<TTrue, TFalse>> a, Boolean<TTrue, TFalse> b) =>
                (Boolean<TTrue, TFalse>)a(b)(False<TTrue, TFalse>);

        // Or = a => b => a(True)(b)
        public static Boolean<TTrue, TFalse> Or<TTrue, TFalse>
            (this Boolean<Boolean<TTrue, TFalse>, Boolean<TTrue, TFalse>> a, Boolean<TTrue, TFalse> b) =>
                (Boolean<TTrue, TFalse>)a(True<TTrue, TFalse>)(b);

        // Not = boolean => boolean(False)(True)
        public static Boolean<TTrue, TFalse> Not<TTrue, TFalse>
            (this Boolean<Boolean<TTrue, TFalse>, Boolean<TTrue, TFalse>> boolean) =>
                (Boolean<TTrue, TFalse>)boolean(False<TTrue, TFalse>)(True<TTrue, TFalse>);

        // Xor = a => b => a(b(False)(True))(b(True)(False))
        public static Boolean<TTrue, TFalse> Xor<TTrue, TFalse>
            (this Boolean<Boolean<TTrue, TFalse>, Boolean<TTrue, TFalse>> a, Boolean<Boolean<TTrue, TFalse>, Boolean<TTrue, TFalse>> b) =>
                (Boolean<TTrue, TFalse>)a((Boolean<TTrue, TFalse>)b(False<TTrue, TFalse>)(True<TTrue, TFalse>))((Boolean<TTrue, TFalse>)b(True<TTrue, TFalse>)(False<TTrue, TFalse>));
    }
}
