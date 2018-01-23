namespace Tutorial.LambdaCalculus
{
    using System.Diagnostics.CodeAnalysis;

#if DEMO
    public delegate TResult Func<TResult>(?);

    public delegate TResult Func<TResult>(Func<TResult> self);
#endif

    public delegate TResult SelfApplicableFunc<TResult>(SelfApplicableFunc<TResult> self);

    [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]
    public static class OmegaCombinators<TResult>
    {
        public static readonly SelfApplicableFunc<TResult>
            ω = f => f(f);

        public static readonly TResult
            Ω = ω(ω);
    }
}