namespace Tutorial.Introduction
{
    using System;

    internal static partial class Functions
    {
        internal static int Add(int a, int b) // Pure.
        {
            return a + b;
        }

        internal static int AddWithLog(int a, int b) // Impure.
        {
            int result = a + b;
            Console.WriteLine("{0} + {1} => {2}", a, b, result);
            return result;
        }
    }

    internal static partial class Functions
    {
        internal static int AddWithLog(int a, int b, Action<int, int, int> logger)
        {
            int result = a + b;
            logger(a, b, result);
            return result;
        }
    }
}
