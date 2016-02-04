namespace System.Linq
{
    internal abstract class EnumerableSorter<TElement>
    {
        internal abstract void ComputeKeys(TElement[] elements, int count);

        internal abstract int CompareKeys(int index1, int index2);

        internal int[] Sort(TElement[] elements, int count)
        {
            this.ComputeKeys(elements, count);
            int[] map = new int[count];
            for (int i = 0; i < count; i++)
            {
                map[i] = i;
            }

            this.QuickSort(map, 0, count - 1);
            return map;
        }

        private void QuickSort(int[] map, int left, int right)
        {
            do
            {
                int i = left;
                int j = right;
                int x = map[i + ((j - i) >> 1)];
                do
                {
                    while (i < map.Length && this.CompareKeys(x, map[i]) > 0)
                    {
                        i++;
                    }

                    while (j >= 0 && this.CompareKeys(x, map[j]) < 0)
                    {
                        j--;
                    }

                    if (i > j)
                    {
                        break;
                    }

                    if (i < j)
                    {
                        int temp = map[i];
                        map[i] = map[j];
                        map[j] = temp;
                    }

                    i++;
                    j--;
                } while (i <= j);

                if (j - left <= right - i)
                {
                    if (left < j)
                    {
                        this.QuickSort(map, left, j);
                    }

                    left = i;
                }
                else
                {
                    if (i < right)
                    {
                        this.QuickSort(map, i, right);
                    }

                    right = j;
                }
            } while (left < right);
        }
    }
}

namespace Dixin.Linq.CategoryTheory
{
    using System.Collections.Generic;
    using System.Linq;

    // [Pure]
    public static partial class EnumerableExtensions
    {
        internal static IEnumerable<T> QuickSort<T>(this IEnumerable<T> source, Comparer<T> comparer = null)
        {
            if (!source.Any())
            {
                return source; // End of recursion.
            }

            comparer = comparer ?? Comparer<T>.Default;
            T head = source.First();
            IEnumerable<T> tail = source.Skip(1);
            IEnumerable<T> smallerThanHead = (from value in tail
                                              where comparer.Compare(value, head) <= 0
                                              select value).QuickSort();
            IEnumerable<T> greaterThanHead = (from value in tail
                                              where comparer.Compare(value, head) > 0
                                              select value).QuickSort();
            return smallerThanHead.Concat(head.Enumerable()).Concat(greaterThanHead);
        }
    }
}
