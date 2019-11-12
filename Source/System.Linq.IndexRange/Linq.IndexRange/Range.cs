namespace System.Linq
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public static partial class EnumerableExtensions
    {
        public static IEnumerable<int> Range(Range range)
        {
            Index startIndex = range.Start;
            Index endIndex = range.End;
            int firstValue = startIndex.FromEnd ? int.MaxValue - startIndex.Value + 1 : startIndex.Value;
            int lastValue = endIndex.FromEnd ? int.MaxValue - endIndex.Value : endIndex.Value - 1;
            if (lastValue < firstValue - 1)
            {
                // ThrowHelper.ThrowOverflowException();
                throw new OverflowException(); // Following the behavior of array with range.
            }

            if (lastValue == firstValue - 1)
            {
                return Enumerable.Empty<int>();
            }

            return RangeIterator(firstValue, lastValue);
        }

        private static IEnumerable<int> RangeIterator(int firstValue, int lastValue)
        {
            for (int value = firstValue; value <= lastValue; value = checked(value + 1))
            {
                yield return value;
                if (value == int.MaxValue)
                {
                    yield break;
                }
            }
        }
    }
}