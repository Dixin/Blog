namespace Dixin.Linq.Lambda
{
    using System;
    using Dixin.Linq.Combinators;

    public static partial class ChurchBoolean
    {
        // Sequence = False
        public static Func<T2, T2> Sequence<T1, T2>
            (T1 arg1) => arg2 => arg2;
    }

    internal abstract class Halting
    {
        // IsHalting = f => x => True or False
        internal abstract Func<T, Boolean> IsHalting<T, TResult>(Func<T, TResult> f);

        // IsNotHalting = f => If(IsHalting(f)(f))(_ => Sequence(Ω())(False))(_ => True)
        internal Boolean IsNotHalting<T>
            (ω<T> f) =>
                ChurchBoolean.If<Boolean>(this.IsHalting(new Func<ω<T>, T>(f))(f))
                    (_ => ChurchBoolean.Sequence<T, Boolean>(OmegaCombinators.Ω<T>())(ChurchBoolean.False))
                    (_ => ChurchBoolean.True);
    }

    internal abstract class Equivalence
    {
        // IsEquivalent = f1 => f2 => True or False
        internal abstract Func<Func<T, TResult>, Boolean> IsEquivalent<T, TResult>(Func<T, TResult> f1);

        // IsHalting = f => x => IsEquivalent(_ => Sequence(f(x))(True))(_ => True)
        internal Func<T, Boolean> IsHalting<T, TResult>
            (Func<T, TResult> f) => x =>
                this.IsEquivalent<T, Boolean>
                    (_ => ChurchBoolean.Sequence<TResult, Boolean>(f(x))(ChurchBoolean.True))
                    (_ => ChurchBoolean.True);
    }
}
