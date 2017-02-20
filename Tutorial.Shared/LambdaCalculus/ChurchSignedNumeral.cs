namespace Tutorial.LambdaCalculus
{
    using System;

    using static ChurchBoolean;
    using static ChurchNumeral;

    // SignedNumeral is the alias of Tuple<Numeral, Numeral>.
    public delegate dynamic SignedNumeral(Boolean f);

    public static partial class ChurchSignedNumeral
    {
        // Sign = n => (n, 0)
        public static readonly Func<Numeral, SignedNumeral>
            Sign = n => new SignedNumeral(ChurchTuple<Numeral, Numeral>.Create(n)(Zero));

        // Negate = signed => signed.Swap()
        public static readonly Func<SignedNumeral, SignedNumeral>
            Negate = signed => new SignedNumeral(new Tuple<Numeral, Numeral>(signed).Swap());

        // Positive = signed => signed.Item1()
        public static readonly Func<SignedNumeral, Numeral>
            Positive = signed => new Tuple<Numeral, Numeral>(signed).Item1();

        // Negative = signed => signed.Item2()
        public static readonly Func<SignedNumeral, Numeral>
            Negative = signed => new Tuple<Numeral, Numeral>(signed).Item2();

        // Format = signed =>
        //    If(positive >= negative)
        //        (_ => (positive - negative, 0))
        //        (_ => (0, negative - positive))
        public static readonly Func<SignedNumeral, SignedNumeral>
            Format = signed =>
                If(signed.Positive().IsGreaterThanOrEqualTo(signed.Negative()))
                    (_ => signed.Positive().Subtract(signed.Negative()).Sign())
                    (_ => signed.Negative().Subtract(signed.Positive()).Sign().Negate());
    }

    public static partial class SignedNumeralExtensions
    {
        public static SignedNumeral Sign(this Numeral n) => ChurchSignedNumeral.Sign(n);

        public static SignedNumeral Negate(this SignedNumeral signed) => ChurchSignedNumeral.Negate(signed);

        public static Numeral Positive(this SignedNumeral signed) => ChurchSignedNumeral.Positive(signed);

        public static Numeral Negative(this SignedNumeral signed) => ChurchSignedNumeral.Negative(signed);

        public static SignedNumeral Format(this SignedNumeral signed) => ChurchSignedNumeral.Format(signed);
    }

    public static partial class ChurchSignedNumeral
    {
        // Add = a => b => (a.Positive() + b.Positive(), a.Negative() + b.Negative()).Format()
        public static readonly Func<SignedNumeral, Func<SignedNumeral, SignedNumeral>>
            Add = a => b => new SignedNumeral(ChurchTuple<Numeral, Numeral>.Create
                   (a.Positive().Add(b.Positive()))
                   (a.Negative().Add(b.Negative())))
               .Format();

        // Subtract = a => b => (a.Positive() + b.Negative(), a.Negative() + b.Positive()).Format()
        public static readonly Func<SignedNumeral, Func<SignedNumeral, SignedNumeral>>
            Subtract = a => b => new SignedNumeral(ChurchTuple<Numeral, Numeral>.Create
                    (a.Positive().Add(b.Negative()))
                    (a.Negative().Add(b.Positive())))
                .Format();

        // Multiply = a => b => (a.Positive() * b.Positive() + a.Negative() * b.Negative(), a.Positive() * b.Negative() + a.Negative() * b.Positive()).Format()
        public static readonly Func<SignedNumeral, Func<SignedNumeral, SignedNumeral>>
            Multiply = a => b => new SignedNumeral(ChurchTuple<Numeral, Numeral>.Create
                    (a.Positive().Multiply(b.Positive()).Add(a.Negative().Multiply(b.Negative())))
                    (a.Positive().Multiply(b.Negative()).Add(a.Negative().Multiply(b.Positive()))))
                .Format();

        // / = dividend => divisor => If(divisor.IsZero())(_ => 0)(_ => dividend.DivideBy(divisor))
        private static readonly Func<Numeral, Func<Numeral, Numeral>> 
            DivideByIgnoreZero = dividend => divisor =>
                If(divisor.IsZero())
                    (_ => Zero)
                    (_ => dividend.DivideBy(divisor));

        // DivideBy = dividend => divisor => (dividend.Positive() / divisor.Positive() + dividend.Negative() / divisor.Negative(), dividend.Positive() / divisor.Negative() + (dividend.Negative() / divisor.Positive()).Format();
        public static readonly Func<SignedNumeral, Func<SignedNumeral, SignedNumeral>>
            DivideBy = dividend => divisor => new SignedNumeral(ChurchTuple<Numeral, Numeral>.Create
                    (DivideByIgnoreZero(dividend.Positive())(divisor.Positive()).Add(DivideByIgnoreZero(dividend.Negative())(divisor.Negative())))
                    (DivideByIgnoreZero(dividend.Positive())(divisor.Negative()).Add(DivideByIgnoreZero(dividend.Negative())(divisor.Positive()))))
                .Format();
    }

    public static partial class SignedNumeralExtensions
    {
        public static SignedNumeral Add(this SignedNumeral a, SignedNumeral b) => ChurchSignedNumeral.Add(a)(b);

        public static SignedNumeral Subtract(this SignedNumeral a, SignedNumeral b) => ChurchSignedNumeral.Subtract(a)(b);

        public static SignedNumeral Multiply(this SignedNumeral a, SignedNumeral b) => ChurchSignedNumeral.Multiply(a)(b);

        public static SignedNumeral DivideBy(this SignedNumeral dividend, SignedNumeral divisor) => ChurchSignedNumeral.DivideBy(dividend)(divisor);
    }
}
