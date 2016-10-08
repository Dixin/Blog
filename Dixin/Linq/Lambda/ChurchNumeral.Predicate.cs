namespace Dixin.Linq.Lambda
{
    public partial class Numeral
    {
        public static bool operator ==(Numeral a, uint b) => a.Unchurch() == b;

        public static bool operator ==(uint a, Numeral b) => a == b.Unchurch();

        public static bool operator !=(Numeral a, uint b) => a.Unchurch() != b;

        public static bool operator !=(uint a, Numeral b) => a != b.Unchurch();
    }

    public partial class Numeral
    {
        public static Boolean operator <=(Numeral a, Numeral b) => a.IsLessOrEqual(b);

        public static Boolean operator >=(Numeral a, Numeral b) => a.IsGreaterOrEqual(b);

        public static Boolean operator <(Numeral a, Numeral b) => a.IsLess(b);

        public static Boolean operator >(Numeral a, Numeral b) => a.IsGreater(b);

        public static Boolean operator ==(Numeral a, Numeral b) => a.AreEqual(b);

        public static Boolean operator !=(Numeral a, Numeral b) => a.AreNotEqual(b);
    }

    public partial class Numeral
    {
        public override int GetHashCode() => this.Unchurch().GetHashCode();

        public override bool Equals(object obj)
        {
            Numeral numeral = obj as Numeral;
            return (object)numeral != null && this.AreEqual(numeral).Unchurch();
        }
    }
}