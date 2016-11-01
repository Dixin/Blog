namespace Dixin.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class EnumerableX
    {
        #region List

        public static IEnumerable<TSource> Insert<TSource>(this IEnumerable<TSource> source, int index, TSource value)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} must be 0 or greater than 0.");
            }

            return InsertGenerator(source, index, value);
        }

        private static IEnumerable<TSource> InsertGenerator<TSource>(
            IEnumerable<TSource> source, int index, TSource value)
        {
            int currentIndex = 0;
            foreach (TSource sourceValue in source)
            {
                if (currentIndex == index)
                {
                    yield return value;
                }
                yield return sourceValue;
                currentIndex = checked(currentIndex + 1);
            }

            if (index == currentIndex)
            {
                yield return value;
            }
            else if (index > currentIndex)
            {
                // mscorlib resource ArgumentOutOfRange_ListInsert.
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    $"Index {index} must be equal to or less than the count of sequance {currentIndex}.");
            }
        }

        public static IEnumerable<TSource> RemoveAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} must be 0 or greater than 0.");
            }

            return RemoveAtGenerator(source, index);
        }

        private static IEnumerable<TSource> RemoveAtGenerator<TSource>(IEnumerable<TSource> source, int index)
        {
            int currentIndex = 0;
            foreach (TSource value in source)
            {
                if (currentIndex != index)
                {
                    yield return value;
                }
                currentIndex = checked(currentIndex + 1);
            }

            if (index >= currentIndex)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index), 
                    $"index {index} must be equal to or less than the last index of source {currentIndex}.");
            }
        }

        public static IEnumerable<TSource> Remove<TSource>(
            this IEnumerable<TSource> source,
            TSource remove,
            IEqualityComparer<TSource> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            bool isRemoved = false;
            foreach (TSource value in source)
            {
                if (!isRemoved && comparer.Equals(value, remove))
                {
                    isRemoved = true;
                }
                else
                {
                    yield return value;
                }
            }
        }

        public static IEnumerable<TSource> RemoveAll<TSource>(
            this IEnumerable<TSource> source,
            TSource remove,
            IEqualityComparer<TSource> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            bool isRemoved = false;
            foreach (TSource value in source)
            {
                if (comparer.Equals(value, remove))
                {
                    isRemoved = true;
                }
                else
                {
                    yield return value;
                }
            }
        }

        public static int IndexOf<TSource>(
            this IEnumerable<TSource> source,
            TSource search,
            IEqualityComparer<TSource> comparer = null,
            int startIndex = 0,
            int? count = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            source = source.Skip(startIndex);
            if (count != null)
            {
                source = source.Take(count.Value);
            }

            int index = checked(0 + startIndex);
            foreach (TSource value in source)
            {
                if (comparer.Equals(value, search))
                {
                    return index;
                }
                index = checked(index + 1);
            }
            return -1;
        }

        public static int LastIndexOf<TSource>(
            this IEnumerable<TSource> source,
            TSource search,
            IEqualityComparer<TSource> comparer = null,
            int startIndex = 0,
            int? count = null)
        {
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            source = source.Skip(startIndex);
            if (count != null)
            {
                source = source.Take(count.Value);
            }

            int lastIndex = -1;
            int index = checked(0 + startIndex);
            foreach (TSource value in source)
            {
                if (comparer.Equals(value, search))
                {
                    lastIndex = index;
                }
                index = checked(index + 1);
            }
            return lastIndex;
        }

        #endregion
    }
}
