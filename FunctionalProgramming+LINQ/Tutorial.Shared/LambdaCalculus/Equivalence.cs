namespace Tutorial.LambdaCalculus
{
    using System;

    using static ChurchBoolean;

    public static partial class Functions<T1, T2>
    {
        public static readonly Func<T1, Func<T2, T2>> 
            Sequence = value1 => value2 => value2;
    }

    internal static class Halting<T, TResult>
    {
        // IsHalting = f => x => True if f halts with x; otherwise, False
        internal static readonly Func<Func<T, TResult>, Func<T, Boolean>>
            IsHalting = f => x => throw new NotImplementedException();

        // IsNotHalting = f => If(IsHalting(f)(f))(_ => Sequence(Ω)(False))(_ => True)
        internal static readonly Func<SelfApplicableFunc<TResult>, Boolean>
            IsNotHalting = f =>
                If(Halting<SelfApplicableFunc<TResult>, TResult>.IsHalting(new Func<SelfApplicableFunc<TResult>, TResult>(f))(f))
                    (_ => Functions<TResult, Boolean>.Sequence(OmegaCombinators<TResult>.Ω)(False))
                    (_ => True);
    }

    internal static class Equivalence<T, TResult>
    {
        // IsEquivalent = f1 => f2 => True if f1 and f2 are equivalent; otherwise, False
        internal static readonly Func<Func<T, TResult>, Func<Func<T, TResult>, Boolean>>
            IsEquivalent = f1 => f2 => throw new NotImplementedException();

        // IsHalting = f => x => IsEquivalent(_ => Sequence(f(x))(True))(_ => True)
        internal static readonly Func<Func<T, TResult>, Func<T, Boolean>>
            IsHalting = f => x => Equivalence<T, Boolean>.IsEquivalent(_ => Functions<TResult, Boolean>.Sequence(f(x))(True))(_ => True);
    }
}
