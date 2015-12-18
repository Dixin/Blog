namespace Dixin.Linq.Lambda
{
    // SignedNumeral is the alias of Tuple<_Numeral, _Numeral>
    public delegate object SignedNumeral(Boolean<_Numeral, _Numeral> f);

    public static partial class ChurchSignedNumeral
    {
        public static _Numeral Zero { get; } = _Numeral.Zero;

        // Sign = numeral => ChurchTuple.Create(numeral, Zero)
        public static SignedNumeral Sign
            (this _Numeral numeral) => new SignedNumeral(ChurchTuple.Create<_Numeral, _Numeral>(numeral)(Zero));

        // Negate = signed => signed.Swap()
        public static SignedNumeral Negate
            (this SignedNumeral signed) => new SignedNumeral(new Tuple<_Numeral, _Numeral>(signed).Swap());

        // Positive = signed => signed.Item1()
        public static _Numeral Positive
            (this SignedNumeral signed) => new Tuple<_Numeral, _Numeral>(signed).Item1();

        // Negative = signed => signed.Item2()
        public static _Numeral Negative
            (this SignedNumeral signed) => new Tuple<_Numeral, _Numeral>(signed).Item2();
    }

    public static partial class ChurchSignedNumeral
    {
        // FormatWithZero = signed => If(positive == negative)(_ => Zero.Sign())(_ => If(positive > negative)(__ => (positive - negative).Sign())(__ => (negative - positive).Sign().Negate()))
        public static SignedNumeral FormatWithZero(this SignedNumeral signed)
        {
            // Just to make the code shorter.
            _Numeral positive = signed.Positive();
            _Numeral negative = signed.Negative();

            return ChurchBoolean.If<SignedNumeral>(positive == negative)
                (_ => Zero.Sign())
                (_ => ChurchBoolean.If<SignedNumeral>(positive > negative)
                    (__ => (positive - negative).Sign())
                    (__ => (negative - positive).Sign().Negate()));
        }

        // Add = a => b => ChurchTuple.Create(a.Positive() + b.Positive())(a.Negative() + b.Negative()).FormatWithZero()
        public static SignedNumeral Add
            (this SignedNumeral a, SignedNumeral b) =>
                new SignedNumeral(ChurchTuple.Create<_Numeral, _Numeral>
                    (a.Positive() + b.Positive())
                    (a.Negative() + b.Negative()))
                .FormatWithZero();

        // Subtract = a => b => ChurchTuple.Create(a.Positive() + b.Negative())(a.Negative() + b.Positive()).FormatWithZero()
        public static SignedNumeral Subtract
            (this SignedNumeral a, SignedNumeral b) =>
                new SignedNumeral(ChurchTuple.Create<_Numeral, _Numeral>
                    (a.Positive() + b.Negative())
                    (a.Negative() + b.Positive()))
                .FormatWithZero();

        // Multiply = a => b => ChurchTuple.Create(a.Positive() * b.Positive() + a.Negative() + b.Negative())(a.Positive() * b.Negative() + a.Negative() * b.Positive()).FormatWithZero()
        public static SignedNumeral Multiply
            (this SignedNumeral a, SignedNumeral b) =>
                new SignedNumeral(ChurchTuple.Create<_Numeral, _Numeral>
                    (a.Positive() * b.Positive() + a.Negative() * b.Negative())
                    (a.Positive() * b.Negative() + a.Negative() * b.Positive()))
                .FormatWithZero();

        // DivideBy = dividend => divisor => ChurchTuple.Create((dividend.Positive() | divisor.Positive()) + (dividend.Negative() | divisor.Negative()))((dividend.Positive() | divisor.Negative()) + (dividend.Negative() | divisor.Positive()))).FormatWithZero();
        public static SignedNumeral DivideBy
            (this SignedNumeral dividend, SignedNumeral divisor) =>
                new SignedNumeral(ChurchTuple.Create<_Numeral, _Numeral>
                    ((dividend.Positive() | divisor.Positive()) + (dividend.Negative() | divisor.Negative()))
                    ((dividend.Positive() | divisor.Negative()) + (dividend.Negative() | divisor.Positive())))
                .FormatWithZero();
    }

    public static partial class _NumeralExtensions
    {
        // DivideByIgnoreZero = dividend => divisor => If(divisor.IsZero())(_ => Zero)(_ => dividend._DivideBy(divisor))
        public static _Numeral DivideByIgnoreZero
            (this _Numeral dividend, _Numeral divisor) =>
                ChurchBoolean.If<_Numeral>(divisor.IsZero())
                    (_ => Zero)
                    (_ => dividend._DivideBy(divisor));
    }

    public partial class _Numeral
    {
        public static _Numeral operator |
            (_Numeral dividend, _Numeral divisor) => dividend.DivideByIgnoreZero(divisor);
    }
}
