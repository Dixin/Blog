namespace Tutorial.LambdaCalculus
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
        public static readonly Func<Boolean<Boolean<T>, Boolean<T>>, Func<Boolean<T>, Boolean<T>>>
            And = a => b => (Boolean<T>)a(b)(False);

        // Or = a => b => a(True)(b)
        public static readonly Func<Boolean<Boolean<T>, Boolean<T>>, Func<Boolean<T>, Boolean<T>>>
            Or = a => b => (Boolean<T>)a(True)(b);

        // Not = boolean => boolean(False)(True)
        public static readonly Func<Boolean<Boolean<T>, Boolean<T>>, Boolean<T>>
            Not = boolean => (Boolean<T>)boolean(False)(True);

        // Xor = a => b => a(b(False)(True))(b(True)(False))
        public static readonly Func<Boolean<Boolean<T>, Boolean<T>>, Func<Boolean<Boolean<T>, Boolean<T>>, Boolean<T>>>
            Xor = a => b => (Boolean<T>)a((Boolean<T>)b(False)(True))((Boolean<T>)b(True)(False));
    }

    public static partial class BooleanExtensions
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

    public static partial class BooleanExtensions
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

#if DEMO
    public abstract partial class Boolean
    {
        public abstract Func<TFalse, object> Invoke<TTrue, TFalse>(TTrue @true);

        public Func<T, T> Invoke<T>(T @true) => @false => (T)this.Invoke<T, T>(@true)(@false);
    }

    public abstract partial class Boolean
    {
        private class TrueBoolean : Boolean
        {
            public override Func<TFalse, object> Invoke<TTrue, TFalse>(TTrue @true) => @false => @true;
        }

        private class FalseBoolean : Boolean
        {
            public override Func<TFalse, object> Invoke<TTrue, TFalse>(TTrue @true) => @false => @false;
        }

        public static Boolean True { get; } = new TrueBoolean();

        public static Boolean False { get; } = new FalseBoolean();
    }

    public static class BooleanExtensions
    {
        // And = a => b => a(b)(False)
        public static Boolean And(this Boolean a, Boolean b) => a.Invoke(b)(False);

        // Or = a => b => a(True)(b)
        public static Boolean Or(this Boolean a, Boolean b) => a.Invoke(True)(b);

        // Not = boolean => boolean(False)(True)
        public static Boolean Not(this Boolean boolean) => boolean.Invoke(False)(True);

        // Xor = a => b => a(b(False)(True))(b(True)(False))
        public static Boolean Xor(this Boolean a, Boolean b) => a.Invoke(b.Invoke(False)(True))(b.Invoke(True)(False));
    }

    public static partial class ChurchBoolean<T>
    {
        public static readonly Func<Boolean, Func<Func<Unit<T>, T>, Func<Func<Unit<T>, T>, T>>>
            If = condition => then => @else => condition.Invoke(then)(@else)(_ => _);
    }

    public static partial class ChurchEncoding
    {
        // System.Boolean to Boolean
        public static Boolean Church(this bool boolean) => boolean ? True : False;

        // Boolean to System.Boolean
        public static bool Unchurch(this Boolean boolean) => boolean.Invoke(true)(false);
    }
#endif
}
