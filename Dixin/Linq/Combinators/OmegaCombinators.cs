namespace Dixin.Linq.Combinators
{
    public delegate T ω<T>(ω<T> ω);

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