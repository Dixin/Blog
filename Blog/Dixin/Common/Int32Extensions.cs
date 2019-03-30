namespace Dixin.Common
{
    using System;

    public static class Int32Extensions
    {
        public static bool IsPrime(this int value)
        {
            if (value <= 1)
            {
                return false;
            }

            if (value <= 3)
            {
                return true;
            }

            if (value % 2 == 0 || value % 3 == 0)
            {
                return false;
            }

            int divisorLimit = (int)Math.Floor(Math.Sqrt(value));
            for (int divisor = 5; divisor <= divisorLimit; divisor += 6)
            {
                if (value % divisor == 0 || value % (divisor + 2) == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
