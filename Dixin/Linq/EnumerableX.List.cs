namespace Dixin.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    public enum ListQueryMode
    {
        Strict = 0,
        Normalize,
        Ignore
    }

    public static partial class EnumerableX
    {
        #region List

        public static IEnumerable<TSource> Insert<TSource>(
            this IEnumerable<TSource> source, int index, TSource insert, ListQueryMode mode = ListQueryMode.Strict)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentOutOfRangeException>(mode != ListQueryMode.Strict || index >= 0);

            if (mode == ListQueryMode.Normalize && index < 0)
            {
                index = 0;
            }

            int currentIndex = -1;
            bool isInserted = false;
            foreach (TSource value in source)
            {
                currentIndex = checked(currentIndex + 1);
                if (currentIndex == index)
                {
                    isInserted = true;
                    yield return insert;
                }

                yield return value;
            }

            if (!isInserted)
            {
                int count = checked(currentIndex + 1);
                switch (mode)
                {
                    case ListQueryMode.Normalize:
                        yield return insert;
                        break;

                    case ListQueryMode.Ignore:
                        if (count == index)
                        {
                            yield return insert;
                        }

                        break;

                    case ListQueryMode.Strict:
                        if (count == index)
                        {
                            yield return insert;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException(
                                nameof(index),
                                // mscorlib resource ArgumentOutOfRange_ListInsert.
                                $"Index {index} must be equal to or less than the count of sequance {count}.");
                        }

                        break;
                }
            }
        }

        public static IEnumerable<TSource> RemoveAt<TSource>(
            this IEnumerable<TSource> source, int index, ListQueryMode mode = ListQueryMode.Strict)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentOutOfRangeException>(mode != ListQueryMode.Strict || index >= 0);

            if (index < 0 && mode == ListQueryMode.Normalize)
            {
                index = 0;
            }

            int currentIndex = -1;
            bool isRemoved = false;
            TSource lastValue = default(TSource);
            bool hasLastValue = false;
            foreach (TSource value in source)
            {
                currentIndex = checked(currentIndex + 1);
                if (currentIndex == index)
                {
                    isRemoved = true;
                }
                else
                {
                    if (!hasLastValue)
                    {
                        hasLastValue = true;
                        lastValue = value;
                    }
                    else
                    {
                        yield return lastValue;
                        lastValue = value;
                    }
                }
            }

            if (isRemoved)
            {
                if (hasLastValue)
                {
                    yield return lastValue;
                }
            }
            else
            {
                switch (mode)
                {
                    case ListQueryMode.Strict:
                        if (index < 0 || index > currentIndex)
                        {
                            throw new ArgumentOutOfRangeException(
                                nameof(index),
                                $"index {index} must be equal to or less than the last index of source {currentIndex}.");
                        }

                        break;
                    case ListQueryMode.Ignore:
                        if ((index < 0 || index > currentIndex) && hasLastValue)
                        {
                            yield return lastValue;
                        }

                        break;
                }
            }
        }

        public static IEnumerable<TSource> Remove<TSource>(
            this IEnumerable<TSource> source,
            TSource remove,
            IEqualityComparer<TSource> comparer = null,
            ListQueryMode mode = ListQueryMode.Strict)
        {
            Contract.Requires<ArgumentNullException>(source != null);

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

            if (!isRemoved && mode == ListQueryMode.Strict)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(remove),
                    $"The specified value is not removed because it is not in {nameof(source)}.");
            }
        }

        public static IEnumerable<TSource> RemoveAll<TSource>(
            this IEnumerable<TSource> source,
            TSource remove,
            IEqualityComparer<TSource> comparer = null,
            ListQueryMode mode = ListQueryMode.Strict)
        {
            Contract.Requires<ArgumentNullException>(source != null);

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

            if (!isRemoved && mode == ListQueryMode.Strict)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(remove), $"The specified value is not removed because it is not in {nameof(source)}.");
            }
        }

        public static int IndexOf<TSource>(
            this IEnumerable<TSource> source,
            TSource search,
            IEqualityComparer<TSource> comparer = null,
            int startIndex = 0,
            int? count = null)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            comparer = comparer ?? EqualityComparer<TSource>.Default;
            source = source.Skip(startIndex);
            if (count != null)
            {
                source = source.Take(count.Value);
            }

            int index = checked(-1 + startIndex);
            foreach (TSource value in source)
            {
                index = checked(index + 1);
                if (comparer.Equals(value, search))
                {
                    return index;
                }
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
            Contract.Requires<ArgumentNullException>(source != null);

            comparer = comparer ?? EqualityComparer<TSource>.Default;
            source = source.Skip(startIndex);
            if (count != null)
            {
                source = source.Take(count.Value);
            }

            int lastIndex = -1;
            int index = checked(-1 + startIndex);
            foreach (TSource value in source)
            {
                index = checked(index + 1);
                if (comparer.Equals(value, search))
                {
                    lastIndex = index;
                }
            }

            return lastIndex;
        }

        #endregion
    }
}
