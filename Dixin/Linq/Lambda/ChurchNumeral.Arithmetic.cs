namespace Dixin.Linq.Lambda
{
    using System;

    using static Numeral;

    public static partial class ChurchNumeral
    {
        // Increase = n => new Numeral(n)
        public static Numeral Increase(this Numeral numeral) => new Numeral(numeral);

        // Add = a => b => a(Increase)(b)
        public static Numeral Add(this Numeral a, Numeral b) => a.Invoke<Numeral>(Increase)(b);
    }

    public static partial class ChurchNumeral
    {
        public static readonly Numeral One = Zero.Increase();

        // ...

        // Decrease = n => f => x => n(g => h => h(g(f)))(_ => x)(_ => _)
        public static Numeral Decrease(this Numeral numeral) => 
            new Numeral<Numeral>(f => x =>
                numeral.Invoke<Func<Func<Numeral, Numeral>, Numeral>>(g => h => h(g(f)))(_ => x)(_ => _))
                (Increase)(Zero);

        // Subtract = a => b => b(Decrease)(a)
        public static Numeral Subtract(this Numeral a, Numeral b) => b.Invoke<Numeral>(Decrease)(a);
    }

    public static partial class ChurchNumeral
    {
        // Multiply = a => b => a(x => b.Add(x))(Zero)
        public static Numeral Multiply(this Numeral a, Numeral b) => a.Invoke<Numeral>(b.Add)(Zero);

        // Power = m => e => e(x => m.Multiply(x))(1)
        public static Numeral Pow(this Numeral mantissa, Numeral exponent) => 
            exponent.Invoke<Numeral>(mantissa.Multiply)(One);
    }

    public partial class Numeral
    {
        public static Numeral operator + (Numeral a, Numeral b) => a.Add(b);

        public static Numeral operator - (Numeral a, Numeral b) => a.Subtract(b);

        public static Numeral operator * (Numeral a, Numeral b) => a.Multiply(b);

        public static Numeral operator ^ (Numeral a, Numeral b) => a.Pow(b);

        public static Numeral operator ++ (Numeral numeral) => numeral.Increase();

        public static Numeral operator -- (Numeral numeral) => numeral.Decrease();
    }

    public partial class Numeral
    {
        public static Numeral operator / (Numeral a, Numeral b) => a._DivideBy(b);
    }
}
