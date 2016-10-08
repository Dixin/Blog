namespace Dixin.Linq.Lambda
{
    using static ChurchBoolean;

    public static partial class ChurchEncoding
    {
        // System.Boolean structure to Boolean function.
        public static Boolean Church(this bool @bool) => @bool ? True : False;

        // Boolean function to System.Boolean structure.
        public static bool Unchurch(this Boolean boolean) => (bool)boolean(true)(false);
    }

    public static partial class ChurchEncoding
    {
        public static Numeral Church(this uint n) => n > 0U ? Church(n - 1U).Increase() : ChurchNumeral.Zero;

        public static uint Unchurch(this Numeral numeral) => (uint)numeral(x => (uint)x + 1U)(0U);

        public static string Visualize(this Numeral numeral) =>
            (string)numeral(x => string.Concat((string)x, "*"))(string.Empty);
    }
}