namespace Dixin.Linq.Lambda
{
    using static ChurchNumeral;

    // SignedNumeral is the alias of Tuple<_Numeral, _Numeral>.
    public delegate object SignedNumeral(Either<Numeral, Numeral> f);

    public static partial class ChurchSignedNumeral
    {
        // Sign = numeral => ChurchTuple.Create(numeral, Zero)
        public static SignedNumeral Sign(this Numeral numeral) => 
            new SignedNumeral(ChurchTuple<Numeral, Numeral>.Create(numeral)(Zero));

        // Negate = signed => signed.Swap()
        public static SignedNumeral Negate(this SignedNumeral signed) => 
            new SignedNumeral(new Tuple<Numeral, Numeral>(signed).Swap());

        // Positive = signed => signed.Item1()
        public static Numeral Positive(this SignedNumeral signed) => new Tuple<Numeral, Numeral>(signed).Item1();

        // Negative = signed => signed.Item2()
        public static Numeral Negative(this SignedNumeral signed) => new Tuple<Numeral, Numeral>(signed).Item2();
    }

    public static partial class ChurchSignedNumeral
    {
        // FormatWithZero = signed => If(positive == negative)(_ => Zero.Sign())(_ => If(positive > negative)(__ => (positive - negative).Sign())(__ => (negative - positive).Sign().Negate()))
        public static SignedNumeral FormatWithZero(this SignedNumeral signed) => 
            ChurchBoolean<SignedNumeral>.If(signed.Positive().Equal(signed.Negative()))
                (_ => Zero.Sign())
                (_ => ChurchBoolean<SignedNumeral>.If(signed.Positive().Greater(signed.Negative()))
                    (__ => signed.Positive().Subtract(signed.Negative()).Sign())
                    (__ => signed.Negative().Subtract(signed.Positive()).Sign().Negate()));

        // Add = a => b => ChurchTuple.Create(a.Positive() + b.Positive())(a.Negative() + b.Negative()).FormatWithZero()
        public static SignedNumeral Add(this SignedNumeral a, SignedNumeral b) =>
            new SignedNumeral(ChurchTuple<Numeral, Numeral>.Create
                (a.Positive().Add(b.Positive()))
                (a.Negative().Add(b.Negative())))
            .FormatWithZero();

        // Subtract = a => b => ChurchTuple.Create(a.Positive() + b.Negative())(a.Negative() + b.Positive()).FormatWithZero()
        public static SignedNumeral Subtract(this SignedNumeral a, SignedNumeral b) =>
            new SignedNumeral(ChurchTuple<Numeral, Numeral>.Create
                (a.Positive().Add(b.Negative()))
                (a.Negative().Add(b.Positive())))
            .FormatWithZero();

        // Multiply = a => b => ChurchTuple.Create(a.Positive() * b.Positive() + a.Negative() + b.Negative())(a.Positive() * b.Negative() + a.Negative() * b.Positive()).FormatWithZero()
        public static SignedNumeral Multiply(this SignedNumeral a, SignedNumeral b) =>
            new SignedNumeral(ChurchTuple<Numeral, Numeral>.Create
                (a.Positive().Multiply(b.Positive()).Add(a.Negative().Multiply(b.Negative())))
                (a.Positive().Multiply(b.Negative()).Add(a.Negative().Multiply(b.Positive()))))
            .FormatWithZero();

        // DivideBy = dividend => divisor => ChurchTuple.Create((dividend.Positive() | divisor.Positive()) + (dividend.Negative() | divisor.Negative()))((dividend.Positive() | divisor.Negative()) + (dividend.Negative() | divisor.Positive()))).FormatWithZero();
        public static SignedNumeral DivideBy(this SignedNumeral dividend, SignedNumeral divisor) =>
            new SignedNumeral(ChurchTuple<Numeral, Numeral>.Create
                (dividend.Positive().DivideByIgnoreZero(divisor.Positive()).Add(dividend.Negative().DivideByIgnoreZero(divisor.Negative())))
                (dividend.Positive().DivideByIgnoreZero(divisor.Negative()).Add(dividend.Negative().DivideByIgnoreZero(divisor.Positive()))))
            .FormatWithZero();
    }

    public static partial class ChurchNumeral
    {
        // DivideByIgnoreZero = dividend => divisor => If(divisor.IsZero())(_ => Zero)(_ => dividend._DivideBy(divisor))
        public static Numeral DivideByIgnoreZero(this Numeral dividend, Numeral divisor) =>
            ChurchBoolean<Numeral>.If(divisor.IsZero())
                (_ => Zero)
                (_ => dividend.DivideBy(divisor));
    }
}
