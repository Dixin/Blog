namespace Dixin.Linq.Lambda
{
    public partial class _Numeral
    {
        public static bool operator ==
            (_Numeral a, uint b) => a._Unchurch() == b;

        public static bool operator ==
            (uint a, _Numeral b) => a == b._Unchurch();

        public static bool operator !=
            (_Numeral a, uint b) => a._Unchurch() != b;

        public static bool operator !=
            (uint a, _Numeral b) => a != b._Unchurch();
    }

    public partial class _Numeral
    {
        public static Boolean operator <=
            (_Numeral a, _Numeral b) => a.IsLessOrEqual(b);

        public static Boolean operator >=
            (_Numeral a, _Numeral b) => a.IsGreaterOrEqual(b);

        public static Boolean operator <
            (_Numeral a, _Numeral b) => a.IsLess(b);

        public static Boolean operator >
            (_Numeral a, _Numeral b) => a.IsGreater(b);

        public static Boolean operator ==
            (_Numeral a, _Numeral b) => a.AreEqual(b);

        public static Boolean operator !=
            (_Numeral a, _Numeral b) => a.AreNotEqual(b);
    }

    public partial class _Numeral
    {
        public override int GetHashCode
            () => this._Unchurch().GetHashCode();

        public override bool Equals(object obj)
        {
            _Numeral numeral = obj as _Numeral;
            if ((object)numeral == null)
            {
                return false;
            }

            return this.AreEqual(numeral)._Unchurch();
        }
    }
}