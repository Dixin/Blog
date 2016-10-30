namespace Dixin.Linq.Lambda
{
    using System;

    using static ChurchBoolean;

    public static partial class ChurchBoolean
    {
        // Sequence is equivalent to False.
        public static Func<T2, T2> Sequence<T1, T2>(T1 value1) => value2 => value2;
    }

    internal static class Halting<T, TResult>
    {
        // IsHalting = f => x => True or False
        internal static readonly Func<Func<T, TResult>, Func<T, Boolean>>
            IsHalting = f => x => 
                { throw new NotImplementedException($"{nameof(True)} if {nameof(f)} halts with {nameof(x)}; otherwise, {nameof(False)}."); };

        // IsNotHalting = f => If(IsHalting(f)(f))(_ => Sequence(Ω)(False))(_ => True)
        internal static readonly Func<SelfApplicableFunc<Func<T, TResult>>, Boolean> 
            IsNotHalting = f =>
                If(Halting<SelfApplicableFunc<Func<T, TResult>>, Func<T, TResult>>.IsHalting(new Func<SelfApplicableFunc<Func<T, TResult>>, Func<T, TResult>>(f))(f))
                    (_ => Sequence<Func<T, TResult>, Boolean>(OmegaCombinators<Func<T, TResult>>.Ω)(False))
                    (_ => True);
    }

    internal static class Equivalence<T, TResult>
    {
        // IsEquivalent = f1 => f2 => True or False
        internal static readonly Func<Func<T, TResult>, Func<Func<T, TResult>, Boolean>>
            IsEquivalent = f1 => f2 => 
                { throw new NotImplementedException($"{nameof(True)} if {nameof(f1)} and {nameof(f2)} are equivalent; otherwse, {nameof(False)}."); };

        // IsHalting = f => x => IsEquivalent(_ => Sequence(f(x))(True))(_ => True)
        internal static readonly Func<Func<T, TResult>, Func<T, Boolean>>
            IsHalting = f => x => Equivalence<T, Boolean>.IsEquivalent(_ => Sequence<TResult, Boolean>(f(x))(True))(_ => True);
    }
}
