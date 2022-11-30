namespace Examples.Common;

public static class Int32Helper
{
    public static bool IsPrime(this int value)
    {
        switch (value)
        {
            case <= 1:
                return false;
            case 2 or 3 or 5:
                return true;
        }

        if (value % 2 == 0 || value % 3 == 0 || value % 5 == 0)
        {
            return false;
        }

        for (int factor = 7; factor * factor <= value; factor += 2)
        {
            if (value % factor == 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool[] GetPrimes(int max)
    {
        bool[] isPrime = new bool[max + 1];
        for (int number = 2; number <= max; number++)
        {
            isPrime[number] = true;
        }

        for (int factor = 2; factor * factor <= max; factor++)
        {
            if (!isPrime[factor])
            {
                continue;
            }

            for (int coefficient = factor; coefficient * factor <= max; coefficient++)
            {
                isPrime[coefficient * factor] = false;
            }
        }

        return isPrime;
    }

    public static int GetPrime(int nth)
    {
        int max = nth > 31
            ? nth * (int)Math.Log2(nth)
            : (1 + nth) * ((int)Math.Log2(nth) +  1); // Or constant 127.
        bool[] isPrime = GetPrimes(max);
        for (int number = 2, count = 0; number <= max; number++)
        {
            if (isPrime[number] && ++count == nth)
            {
                return number;
            }
        }

        throw new InvalidOperationException($"{max} is insufficient for {nth}.");
    }
}