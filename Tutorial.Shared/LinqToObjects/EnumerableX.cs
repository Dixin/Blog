namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class EnumerableX
    {
    }

    public static partial class EnumerableX
    {
        #region Generation

        public static IEnumerable<TResult> Create<TResult>(Func<TResult> valueFactory, int? count = null)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            IEnumerable<TResult> CreateGenerator()
            {
                if (count == null)
                {
                    while (true)
                    {
                        yield return valueFactory();
                    }
                }
                for (int index = 0; index < count; index++)
                {
                    yield return valueFactory(); // Deferred execution.
                }
            }
            return CreateGenerator();
        }

        public static IEnumerable<int> RandomInt32(int min = int.MinValue, int max = int.MaxValue, Random random = null)
        {
            random = random ?? new Random();
            return Create(() => random.Next(min, max));
        }

        public static IEnumerable<int> RandomInt32(int min, int max, int seed) =>
            RandomInt32(min, max, new Random(seed));

        public static IEnumerable<double> RandomDouble(int? seed = null)
        {
            Random random = new Random(seed ?? Environment.TickCount);
            return Create(random.NextDouble);
        }

        public static IEnumerable<TResult> FromValue<TResult>(TResult value)
        {
            yield return value;
        }

        public static IEnumerable<TResult> FromValues<TResult>(params TResult[] values) => values;

        public static IEnumerable<TSource> EmptyIfNull<TSource>(this IEnumerable<TSource> source) =>
            source ?? Enumerable.Empty<TSource>();

        #endregion

        #region Concatenation

        public static IEnumerable<TSource> Join<TSource>(this IEnumerable<TSource> source, TSource separator)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    yield return iterator.Current;
                    while (iterator.MoveNext())
                    {
                        yield return separator;
                        yield return iterator.Current;
                    }
                }
            }
        }

        public static IEnumerable<TSource> Join<TSource>(
            this IEnumerable<TSource> source, IEnumerable<TSource> separator)
        {
            separator = separator ?? Enumerable.Empty<TSource>();
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    yield return iterator.Current;
                    while (iterator.MoveNext())
                    {
                        foreach (TSource value in separator)
                        {
                            yield return value;
                        }

                        yield return iterator.Current;
                    }
                }
            }
        }

        public static IEnumerable<TSource> Append<TSource>(
            this IEnumerable<TSource> source, params TSource[] append) =>
                source.Concat(append);

        public static IEnumerable<TSource> Prepend<TSource>(
            this IEnumerable<TSource> source, params TSource[] prepend) =>
                prepend.Concat(source);

        public static IEnumerable<TSource> AppendTo<TSource>(
            this TSource append, IEnumerable<TSource> source) =>
                source.Append(append);

        public static IEnumerable<TSource> PrependTo<TSource>(
            this TSource prepend, IEnumerable<TSource> source) =>
                source.Prepend(prepend);

        #endregion

        #region Partitioning

        public static IEnumerable<TSource> Subsequence<TSource>(
            this IEnumerable<TSource> source, int startIndex, int count) =>
                source.Skip(startIndex).Take(count);

        #endregion

        #region Aggregation

        public static double VariancePopulation<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IFormatProvider formatProvider = null)
            where TKey : IConvertible
        {
            // Excel VAR.P function:
            // https://support.office.com/en-us/article/VAR-P-function-73d1285c-108c-4843-ba5d-a51f90656f3a
            double[] keys = source.Select(key => keySelector(key).ToDouble(formatProvider)).ToArray();
            double mean = keys.Average();
            return keys.Sum(key => (key - mean) * (key - mean)) / keys.Length;
        }

        public static double VarianceSample<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IFormatProvider formatProvider = null)
            where TKey : IConvertible
        {
            // Excel VAR.S function:
            // https://support.office.com/en-us/article/VAR-S-function-913633de-136b-449d-813e-65a00b2b990b
            double[] keys = source.Select(key => keySelector(key).ToDouble(formatProvider)).ToArray();
            double mean = keys.Average();
            return keys.Sum(key => (key - mean) * (key - mean)) / (keys.Length - 1);
        }

        public static double Variance<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IFormatProvider formatProvider = null)
            where TKey : IConvertible =>
                // Excel VAR function:
                // https://support.office.com/en-us/article/VAR-function-270da762-03d5-4416-8503-10008194458a
                source.VarianceSample(keySelector, formatProvider);

        public static double StandardDeviationPopulation<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IFormatProvider formatProvider = null)
            where TKey : IConvertible =>
                // Excel STDEV.P function: 
                // https://support.office.com/en-us/article/STDEV-P-function-6e917c05-31a0-496f-ade7-4f4e7462f285
                Math.Sqrt(source.VariancePopulation(keySelector, formatProvider));

        public static double StandardDeviationSample<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IFormatProvider formatProvider = null)
            where TKey : IConvertible =>
                // Excel STDEV.S function:
                // https://support.office.com/en-us/article/STDEV-S-function-7d69cf97-0c1f-4acf-be27-f3e83904cc23
                Math.Sqrt(source.VarianceSample(keySelector, formatProvider));

        public static double StandardDeviation<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IFormatProvider formatProvider = null)
            where TKey : IConvertible =>
                // Excel STDDEV.P function:
                // https://support.office.com/en-us/article/STDEV-function-51fecaaa-231e-4bbb-9230-33650a72c9b0
                Math.Sqrt(source.Variance(keySelector, formatProvider));

        public static double PercentileExclusive<TSource, TKey>( // Excel PERCENTILE.EXC function.
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            double percentile,
            IComparer<TKey> comparer = null,
            IFormatProvider formatProvider = null)
            where TKey : IConvertible
        {
            if (percentile < 0 || percentile > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(percentile), $"{nameof(percentile)} must be between 0 and 1.");
            }

            // Excel PERCENTILE.EXC function:
            // https://support.office.com/en-us/article/PERCENTILE-EXC-function-bbaa7204-e9e1-4010-85bf-c31dc5dce4ba
            comparer = comparer ?? Comparer<TKey>.Default;
            TKey[] orderedKeys = source.Select(keySelector).OrderBy(key => key, comparer).ToArray();
            int length = orderedKeys.Length;
            if (percentile < (double)1 / length || percentile > 1 - (double)1 / (length + 1))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(percentile),
                    $"{nameof(percentile)} must be in the range between (1 / source.Count()) and (1 - 1 / source.Count()).");
            }

            double index = percentile * (length + 1) - 1;
            int integerComponentOfIndex = (int)index;
            double decimalComponentOfIndex = index - integerComponentOfIndex;
            double keyAtIndex = orderedKeys[integerComponentOfIndex].ToDouble(formatProvider);

            double keyAtNextIndex = orderedKeys[integerComponentOfIndex + 1].ToDouble(formatProvider);
            return keyAtIndex + (keyAtNextIndex - keyAtIndex) * decimalComponentOfIndex;
        }

        public static double PercentileInclusive<TSource, TKey>( // Excel PERCENTILE.INC function.
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            double percentile,
            IComparer<TKey> comparer = null,
            IFormatProvider formatProvider = null)
            where TKey : IConvertible
        {
            if (percentile < 0 || percentile > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(percentile), $"{nameof(percentile)} must be between 0 and 1.");
            }

            // Excel PERCENTILE.INC function:
            // https://support.office.com/en-us/article/PERCENTILE-INC-Function-DAX-15f69af8-1588-4863-9acf-2acc00384ffd
            comparer = comparer ?? Comparer<TKey>.Default;
            TKey[] orderedKeys = source.Select(keySelector).OrderBy(key => key, comparer).ToArray();
            int length = orderedKeys.Length;

            double index = percentile * (length - 1);
            int integerComponentOfIndex = (int)index;
            double decimalComponentOfIndex = index - integerComponentOfIndex;
            double keyAtIndex = orderedKeys[integerComponentOfIndex].ToDouble(formatProvider);

            if (integerComponentOfIndex >= length - 1)
            {
                return keyAtIndex;
            }

            double keyAtNextIndex = orderedKeys[integerComponentOfIndex + 1].ToDouble(formatProvider);
            return keyAtIndex + (keyAtNextIndex - keyAtIndex) * decimalComponentOfIndex;
        }

        public static double Percentile<TSource, TKey>( // Excel PERCENTILE function.
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            double percentile,
            IComparer<TKey> comparer = null,
            IFormatProvider formatProvider = null)
            where TKey : IConvertible
        {
            if (percentile < 0 || percentile > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(percentile), $"{nameof(percentile)} must be between 0 and 1.");
            }

            // Excel PERCENTILE function:
            // https://support.office.com/en-us/article/PERCENTILE-function-91b43a53-543c-4708-93de-d626debdddca
            // https://en.wikipedia.org/wiki/Percentile#Definition_of_the_Microsoft_Excel_method
            return PercentileInclusive(source, keySelector, percentile, comparer, formatProvider);
        }

        #endregion

        #region Quantifiers

        public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> source) => source == null || !source.Any();

        public static bool IsNotNullOrEmpty<TSource>(this IEnumerable<TSource> source) => source != null && source.Any();

        #endregion

        #region Iteration

        public static void ForEach<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> onNext)
        {
            foreach (TSource value in source)
            {
                if (!onNext(value))
                {
                    break;
                }
            }
        }

        public static void ForEach<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> onNext)
        {
            int index = 0;
            foreach (TSource value in source)
            {
                if (!onNext(value, index))
                {
                    break;
                }

                index = checked(index + 1); // Not checked in the source code.
            }
        }

        public static void ForEach<TSource>(this IEnumerable<TSource> source)
        {
            using (IEnumerator<TSource> iterator = source.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                }
            }
        }

        public static void ForEach(this IEnumerable source)
        {
            IEnumerator iterator = source.GetEnumerator();
            while (iterator.MoveNext())
            {
            }
        }

        #endregion
    }
}
