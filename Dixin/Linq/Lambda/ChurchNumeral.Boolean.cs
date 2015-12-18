namespace Dixin.Linq.Lambda
{
    public static partial class _NumeralExtensions
    {
        // _DivideBy = dividend => divisor => 
        // If(dividend.IsGreaterOrEqual(divisor))
        //    (_ => One + (dividend - divisor)._DivideBy(divisor))
        //    (_ => Zero);
        public static _Numeral _DivideBy
            (this _Numeral dividend, _Numeral divisor) =>
                ChurchBoolean.If<_Numeral>(dividend >= divisor)
                    (_ => One + (dividend - divisor)._DivideBy(divisor))
                    (_ => Zero);
    }
}
