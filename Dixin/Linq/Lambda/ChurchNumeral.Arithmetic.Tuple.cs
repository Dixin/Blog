namespace Dixin.Linq.Lambda
{
    public static partial class _NumeralExtensions
    {
        // Decrease2 = n => n(tuple => tuple.Shift(Increase))(ChurchTuple.Create(Zero)(Zero)).Item1();
        public static _Numeral Decrease2
            (this _Numeral numeral) =>
                numeral.Numeral<Tuple<_Numeral, _Numeral>>()
                    (tuple => tuple.Shift(Increase)) // (x, y) -> (y, y + 1)
                    (ChurchTuple.Create<_Numeral, _Numeral>(Zero)(Zero))
                .Item1();
    }
}
