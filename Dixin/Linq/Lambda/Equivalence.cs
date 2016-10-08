namespace Dixin.Linq.Lambda
{
    using System;

    using Dixin.Linq.Combinators;

    using static ChurchBoolean;

    public static partial class ChurchBoolean
    {
        // Sequence is equivalent to False.
        public static Func<T2, T2> Sequence<T1, T2>(T1 value1) => value2 => value2;
    }

    internal abstract class Halting
    {
        // IsHalting = f => x => True or False
        internal abstract Func<T, Boolean> IsHalting<T, TResult>(Func<T, TResult> f);

        // IsNotHalting = f => If(IsHalting(f)(f))(_ => Sequence(Ω())(False))(_ => True)
        internal Boolean IsNotHalting<T>
            (ω<T> f) =>
                ChurchBoolean<Boolean>.If(this.IsHalting(new Func<ω<T>, T>(f))(f))
                    (_ => Sequence<T, Boolean>(OmegaCombinators<T>.Ω(__ => __))(False))
                    (_ => True);
    }

    internal abstract class Equivalence
    {
        // IsEquivalent = f1 => f2 => True or False
        internal abstract Func<Func<T, TResult>, Boolean> IsEquivalent<T, TResult>(Func<T, TResult> f1);

        // IsHalting = f => x => IsEquivalent(_ => Sequence(f(x))(True))(_ => True)
        internal Func<T, Boolean> IsHalting<T, TResult>
            (Func<T, TResult> f) => x =>
                this.IsEquivalent<T, Boolean>
                    (_ => Sequence<TResult, Boolean>(f(x))(True))
                    (_ => True);
    }
}
