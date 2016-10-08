namespace Dixin.Linq.Combinators
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Dixin.Linq.Lambda;

    public delegate T ω<T>(ω<T> ω);

    [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]
    public static class OmegaCombinators<T>
    {
        // ω = x => x(x)
        public static readonly ω<T>
            ω = x => x(x);

        // Ω = ω(ω)
        public static readonly Func<Unit<T>, T> 
            Ω = _ => ω(ω);  // Ω = ω(ω) start execution immediately and throws exception. Ω = _ => ω(ω) defers the execution.
    }
}