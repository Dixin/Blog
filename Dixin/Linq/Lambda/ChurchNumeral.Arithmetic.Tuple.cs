namespace Dixin.Linq.Lambda
{
    using static Numeral;

    public static partial class ChurchNumeral
    {
        // Decrease2 = n => n(tuple => tuple.Shift(Increase))(ChurchTuple.Create(Zero)(Zero)).Item1();
        public static Numeral Decrease2(this Numeral numeral) =>
            numeral.Invoke<Tuple<Numeral, Numeral>>
                (tuple => tuple.Shift(Increase)) // (x, y) -> (y, y + 1)
                (ChurchTuple<Numeral, Numeral>.Create(Zero)(Zero))
            .Item1();
    }
}
