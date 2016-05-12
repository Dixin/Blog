namespace Dixin.Linq.Parallel
{
    using System.Linq;

    internal static class HelperMethods
    {
        internal static int Compute(int value = 0, int iteration = 10000000)
        {
            Enumerable.Range(0, iteration * (value + 1)).ForEach();
            return value;
        }
    }
}
