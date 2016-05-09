namespace Dixin.Linq.Parallel
{
    using System.Linq;

    internal static partial class HelperMethods
    {
        internal static int Compute(int value = 0, int iteration = 10000000)
        {
            Enumerable.Range(0, iteration * (value + 1)).ForEach();
            return value;
        }
    }

    internal static partial class HelperMethods
    {
        internal static int[][] RandomArrays
            (int length, int count, int minValue = int.MinValue, int maxValue = int.MaxValue)
                => Enumerable
                    .Range(0, count)
                    .Select(_ => EnumerableX.RandomInt32(minValue, maxValue).Take(length).ToArray())
                    .ToArray();
    }
}
