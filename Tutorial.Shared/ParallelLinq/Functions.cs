namespace Tutorial.ParallelLinq
{
    using System.Linq;

    internal static class Functions
    {
        internal static int Compute(int value = 0, int iteration = 10_000_000)
        {
            Enumerable.Range(0, iteration * (value + 1)).ForEach();
            return value;
        }
    }
}
