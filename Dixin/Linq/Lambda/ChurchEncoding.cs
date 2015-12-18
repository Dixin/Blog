namespace Dixin.Linq.Lambda
{
    public static partial class ChurchEncoding
    {
        // System.Boolean to Boolean
        public static Boolean _Church
            (this bool boolean) => boolean ? new Boolean(ChurchBoolean.True) : ChurchBoolean.False;

        // Boolean to System.Boolean
        public static bool _Unchurch
            (this Boolean boolean) => (bool)boolean(true)(false);
    }

    public static partial class ChurchEncoding
    {
        public static _Numeral _Church
            (this uint n) => n > 0 ? new _Numeral(_Church(n - 1)) : _Numeral.Zero;

        public static uint _Unchurch
            (this _Numeral numeral) => numeral.Numeral<uint>()(x => x + 1)(0);
    }

    public static partial class ChurchEncoding
    {
        public static string Visualize
        (this _Numeral numeral) =>
            numeral.Numeral<string>()(x => string.Concat(x, "#"))(string.Empty);
    }
}