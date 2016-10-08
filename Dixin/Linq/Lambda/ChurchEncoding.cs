namespace Dixin.Linq.Lambda
{
    using static ChurchBoolean;
    using static Numeral;

    public static partial class ChurchEncoding
    {
        // System.Boolean structure to Boolean function.
        public static Boolean Church(this bool @bool) => @bool ? True : False;

        // Boolean function to System.Boolean structure.
        public static bool Unchurch(this Boolean boolean) => (bool)boolean(true)(false);
    }

    public static partial class ChurchEncoding
    {
        public static Numeral Church(this uint n) => n > 0 ? new Numeral(Church(n - 1)) : Zero;

        public static uint Unchurch(this Numeral numeral) => numeral.Invoke<uint>(x => x + 1)(0);
    }

    public static partial class ChurchEncoding
    {
        public static string Visualize(this Numeral numeral) => 
            numeral.Invoke<string>(x => string.Concat(x, "*"))(string.Empty);
    }
}