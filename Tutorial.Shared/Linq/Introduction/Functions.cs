namespace Dixin.Linq.Introduction
{
    using System;

    internal static class Functions
    {
        internal static int Add(int a, int b)
        {
            return a + b;
        }

        internal static void AddToConsole(int a, int b)
        {
            Console.WriteLine("{0} => {1}", DateTime.Now.ToString("o"), a + b);
        }
    }
}
