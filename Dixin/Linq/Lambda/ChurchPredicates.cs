namespace Dixin.Linq.Lambda
{
    using System.Diagnostics.CodeAnalysis;

    using static ChurchBoolean;

    public static partial class ChurchPredicates
    {
        // IsZero = n => n(_ => False)(True)
        public static Boolean IsZero(this Numeral numeral) =>
            numeral.Invoke<Boolean>(_ => False)(True);
    }

    public static partial class ChurchPredicates
    {
        // IsLessOrEqual = a => b => a.Subtract(b).IsZero()
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LessOr")]
        public static Boolean IsLessOrEqual(this Numeral a, Numeral b) => a.Subtract(b).IsZero();

        // IsGreaterOrEqual = a => b => b.Subtract(a).IsZero()
        public static Boolean IsGreaterOrEqual(this Numeral a, Numeral b) => b.Subtract(a).IsZero();

        // IsLess = a => b => a.IsGreaterOrEqual(b).Not()
        public static Boolean IsLess(this Numeral a, Numeral b) => a.IsGreaterOrEqual(b).Not();

        // IsGreater = a => b => a.IsLessOrEqual(b).Not()
        public static Boolean IsGreater(this Numeral a, Numeral b) => a.IsLessOrEqual(b).Not();

        // AreEqual = a => b => a.Subtract(b).IsZero().And(a.Subtract(b).IsZero())
        // Or:
        // AreEqual = a => b => a.IsLessOrEqual(b).And(a.IsGreaterOrEqual(b))
        public static Boolean AreEqual(this Numeral a, Numeral b) => a.IsLessOrEqual(b).And(a.IsGreaterOrEqual(b));

        // AreNotEqual = a => b => a.AreEqual(b).Not()
        public static Boolean AreNotEqual(this Numeral a, Numeral b) => a.AreEqual(b).Not();
    }
}