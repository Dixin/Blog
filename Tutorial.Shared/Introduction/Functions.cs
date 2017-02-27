namespace Tutorial.Introduction
{
    using System;

    using Tutorial.CategoryTheory;

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

        internal static void AddWithCallback(int a, int b, Action<int> callback = null)
        {
            callback = callback ?? (value => Console.WriteLine(
                "{0} => {1}", DateTime.Now.ToString("o"), value));
            callback(a + b);
        }

        internal static Cps<TContinuation, int> AddCps<TContinuation>(int a, int b)
        {
            return continuation => continuation(a + b);
        }
    }
}
