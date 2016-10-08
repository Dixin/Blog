namespace Dixin.Linq.Lambda
{
    using System.Diagnostics.CodeAnalysis;

    using static ChurchBoolean;

    public static partial class ChurchPredicates
    {
        // IsZero = n => n(_ => False)(True)
        public static Boolean IsZero(this Numeral numeral) => (Boolean)numeral(_ => False)(True);
    }

    public static partial class ChurchPredicates
    {
        // IsLessOrEqual = a => b => a.Subtract(b).IsZero()
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LessOr")]
        public static Boolean LessOrEqual(this Numeral a, Numeral b) => a.Subtract(b).IsZero();

        // IsGreaterOrEqual = a => b => b.Subtract(a).IsZero()
        public static Boolean GreaterOrEqual(this Numeral a, Numeral b) => b.Subtract(a).IsZero();

        // IsLess = a => b => a.IsGreaterOrEqual(b).Not()
        public static Boolean Less(this Numeral a, Numeral b) => a.GreaterOrEqual(b).Not();

        // IsGreater = a => b => a.IsLessOrEqual(b).Not()
        public static Boolean Greater(this Numeral a, Numeral b) => a.LessOrEqual(b).Not();

        // AreEqual = a => b => a.Subtract(b).IsZero().And(a.Subtract(b).IsZero())
        // Or:
        // AreEqual = a => b => a.IsLessOrEqual(b).And(a.IsGreaterOrEqual(b))
        public static Boolean Equal(this Numeral a, Numeral b) => a.LessOrEqual(b).And(a.GreaterOrEqual(b));

        // AreNotEqual = a => b => a.AreEqual(b).Not()
        public static Boolean NotEqual(this Numeral a, Numeral b) => a.Equal(b).Not();
    }
}