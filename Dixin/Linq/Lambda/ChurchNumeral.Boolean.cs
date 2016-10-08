namespace Dixin.Linq.Lambda
{
    using static Numeral;

    public static partial class ChurchNumeral
    {
        // _DivideBy = dividend => divisor => 
        // If(dividend.IsGreaterOrEqual(divisor))
        //    (_ => One + (dividend - divisor)._DivideBy(divisor))
        //    (_ => Zero);
        public static Numeral _DivideBy(this Numeral dividend, Numeral divisor) =>
            ChurchBoolean<Numeral>.If(dividend >= divisor)
                (_ => One + (dividend - divisor)._DivideBy(divisor))
                (_ => Zero);
    }
}
