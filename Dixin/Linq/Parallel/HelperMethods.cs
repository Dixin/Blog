namespace Dixin.Linq.Parallel
{
    using System.Linq;

    internal static partial class HelperMethods
    {
        private const int IterationCount = 10000000;

        internal static int Computing(int value = 0)
        {
            Enumerable.Range(0, (value + 1)*IterationCount).ForEach();
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