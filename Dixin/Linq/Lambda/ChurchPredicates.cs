namespace Dixin.Linq.Lambda
{
    public static partial class ChurchPredicates
    {
        // IsZero = n => n(_ => False)(True)
        public static Boolean IsZero
            (this _Numeral numeral) =>
                numeral.Numeral<Boolean>()(_ => ChurchBoolean.False)(ChurchBoolean.True);
    }

    public static partial class ChurchPredicates
    {
        // IsLessOrEqual = a => b => a.Subtract(b).IsZero()
        public static Boolean IsLessOrEqual
            (this _Numeral a, _Numeral b) => a.Subtract(b).IsZero();

        // IsGreaterOrEqual = a => b => b.Subtract(a).IsZero()
        public static Boolean IsGreaterOrEqual
            (this _Numeral a, _Numeral b) => b.Subtract(a).IsZero();

        // IsLess = a => b => a.IsGreaterOrEqual(b).Not()
        public static Boolean IsLess
            (this _Numeral a, _Numeral b) => a.IsGreaterOrEqual(b).Not();

        // IsGreater = a => b => a.IsLessOrEqual(b).Not()
        public static Boolean IsGreater
            (this _Numeral a, _Numeral b) => a.IsLessOrEqual(b).Not();

        // AreEqual = a => b => a.Subtract(b).IsZero().And(a.Subtract(b).IsZero())
        // Or:
        // AreEqual = a => b => a.IsLessOrEqual(b).And(a.IsGreaterOrEqual(b))
        public static Boolean AreEqual
            (this _Numeral a, _Numeral b) => a.IsLessOrEqual(b).And(a.IsGreaterOrEqual(b));

        // AreNotEqual = a => b => a.AreEqual(b).Not()
        public static Boolean AreNotEqual
            (this _Numeral a, _Numeral b) => a.AreEqual(b).Not();
    }
}