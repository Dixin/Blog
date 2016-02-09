namespace Dixin.Linq.Combinators
{
    using System.Diagnostics.CodeAnalysis;

    public delegate T ω<T>(ω<T> ω);

    [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]
    public static class OmegaCombinators
    {
        // ω = x => x(x)
        public static T ω<T>
            (ω<T> x) => x(x);

        // Ω = ω(ω)
        public static T Ω<T>
            () => ω<T>(ω); // Ω<T> = ω<T>(ω) throws exception.
    }
}