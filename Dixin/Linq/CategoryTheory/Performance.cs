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
                }
                while (i <= j);

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
            }
            while (left < right);
        }
    }
}

namespace Dixin.Linq.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using CustomLinq = Dixin.Linq.LinqToObjects.EnumerableExtensions;

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
            IEnumerable<T> smallerThanHead =
                (from value in tail
                 where comparer.Compare(value, head) <= 0
                 select value).QuickSort(); // Recursion.
            IEnumerable<T> greaterThanHead =
                (from value in tail
                 where comparer.Compare(value, head) > 0
                 select value).QuickSort(); // Recursion.
            return smallerThanHead.Concat(head.Enumerable()).Concat(greaterThanHead);
        }
    }

    // Impure.
    public static class StopwatchHelper
    {
        public const int DefaultCount = 100;

        private static readonly Stopwatch DefaultStopwatch = new Stopwatch();

        public static long Run(this Action action, int count = DefaultCount, Stopwatch stopwatch = null)
        {
            stopwatch = stopwatch ?? DefaultStopwatch;
            stopwatch.Reset();
            action(); // Warm up.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            stopwatch.Start();

            for (int index = 0; index < count; index++)
            {
                action();
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        public static long RunEach<T>
            (this IEnumerable<T> args, Func<T, T> action, int count = DefaultCount, Stopwatch stopwatch = null) =>
                Run(() => args.ForEach(arg => action(arg)), count);

        public static long RunEach<T1, T2>
            (this IEnumerable<IEnumerable<T1>> args1,
            Func<IEnumerable<T1>, Func<T1, T2>, IEnumerable<T1>> action,
            Func<T1, T2> arg2,
            int count = DefaultCount,
            Stopwatch stopwatch = null)
                => Run(() => args1.ForEach(arg1 => action(arg1, arg2).ForEach()), count);

        public static long Run<T>(this T arg, Func<T, T> action, int count = DefaultCount, Stopwatch stopwatch = null) =>
            Run(() => action(arg), count);

        public static long Run<T1, T2>
            (this IEnumerable<T1> arg1,
            Func<IEnumerable<T1>, Func<T1, T2>, IEnumerable<T1>> action,
            Func<T1, T2> arg2,
            int count = DefaultCount,
            Stopwatch stopwatch = null)
                => Run(() => action(arg1, arg2).ForEach(), count);
    }

    public static class ArrayHelper
    {
        public static int[][] RandomArrays(int minValue, int maxValue, int minLength, int maxLength, int count)
            => Enumerable
                .Range(0, count)
                .Select(_ => RandomArray(minValue, maxValue, minLength, maxLength))
                .ToArray();

        public static int[] RandomArray(int minValue, int maxValue, int minLength, int maxLength)
        {
            Random random = new Random();
            return EnumerableX
                .RandomInt32(minValue, maxValue, random).Take(random.Next(minLength, maxLength))
                .ToArray();
        }
    }

    public class PersonReferenceType : IComparable<PersonReferenceType>
    {
        private static readonly string LongString =
            Enumerable.Range(0, 10000).Select(_ => Guid.NewGuid().ToString()).Aggregate(string.Concat);

        public string Name { get; private set; }

        public int Age { get; private set; }

        public string Description { get; private set; }

        public static IEnumerable<PersonReferenceType> Random(int count)
        {
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                yield return new PersonReferenceType()
                {
                    Name = Guid.NewGuid().ToString(),
                    Age = random.Next(0, 100),
                    Description = LongString
                };
            }
        }

        public int CompareTo(PersonReferenceType other)
        {
            int nameCompare = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
            return nameCompare != 0 ? nameCompare : this.Age.CompareTo(other.Age);
        }
    }

    public struct PersonValueType : IComparable<PersonValueType>
    {
        private static readonly string LongString =
            Enumerable.Range(0, 10000).Select(_ => Guid.NewGuid().ToString()).Aggregate(string.Concat);

        public string Name { get; private set; }

        public int Age { get; private set; }

        public string Description { get; private set; }

        public static IEnumerable<PersonValueType> Random(int count)
        {
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                yield return new PersonValueType()
                {
                    Name = Guid.NewGuid().ToString(),
                    Age = random.Next(0, 100),
                    Description = LongString
                };
            }
        }

        public int CompareTo(PersonValueType other)
        {
            int nameCompare = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
            return nameCompare != 0 ? nameCompare : this.Age.CompareTo(other.Age);
        }
    }

    // Impure.
    internal static partial class Sort
    {
        internal static T[] ArraySort<T>(T[] array)
        {
            Array.Sort(array);
            return array;
        }

        internal static T[] LinqOrderBy<T>(T[] array) => array.OrderBy(value => value).ToArray();

        internal static T[] CustomLinqOrderBy<T>(T[] array) => CustomLinq.OrderBy(array, value => value).ToArray();

        internal static T[] FunctionalQuickSort<T>(T[] array) => array.QuickSort().ToArray();
    }

    // Impure.
    internal static partial class Sort
    {
        internal static void Int32Array()
        {
            int[][] arrays1 = ArrayHelper.RandomArrays(int.MinValue, int.MaxValue, 0, 100, 100);
            int[][] arrays2 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            int[][] arrays3 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            int[][] arrays4 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            Trace.WriteLine($"{nameof(ArraySort)}: {arrays1.RunEach(ArraySort)}");
            Trace.WriteLine($"{nameof(LinqOrderBy)}: {arrays2.RunEach(LinqOrderBy)}");
            Trace.WriteLine($"{nameof(CustomLinqOrderBy)}: {arrays4.RunEach(CustomLinqOrderBy)}");
            Trace.WriteLine($"{nameof(FunctionalQuickSort)}: {arrays3.RunEach(FunctionalQuickSort)}");
        }

        internal static void StringArray()
        {
            string[] array1 = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid().ToString()).ToArray();
            string[] array2 = array1.ToArray(); // Copy.
            string[] array3 = array1.ToArray(); // Copy.
            string[] array4 = array1.ToArray(); // Copy.
            Trace.WriteLine($"{nameof(ArraySort)}: {array1.Run(ArraySort)}");
            Trace.WriteLine($"{nameof(LinqOrderBy)}: {array2.Run(LinqOrderBy)}");
            Trace.WriteLine($"{nameof(CustomLinqOrderBy)}: {array4.Run(CustomLinqOrderBy)}");
            Trace.WriteLine($"{nameof(FunctionalQuickSort)}: {array3.Run(FunctionalQuickSort)}");
        }

        internal static void ValueTypeArray()
        {
            PersonValueType[] array1 = PersonValueType.Random(100).ToArray();
            PersonValueType[] array2 = array1.ToArray(); // Copy.
            PersonValueType[] array3 = array1.ToArray(); // Copy.
            PersonValueType[] array4 = array1.ToArray(); // Copy.
            Trace.WriteLine($"{nameof(ArraySort)}: {array1.Run(ArraySort)}");
            Trace.WriteLine($"{nameof(LinqOrderBy)}: {array2.Run(LinqOrderBy)}");
            Trace.WriteLine($"{nameof(CustomLinqOrderBy)}: {array4.Run(CustomLinqOrderBy)}");
            Trace.WriteLine($"{nameof(FunctionalQuickSort)}: {array3.Run(FunctionalQuickSort)}");
        }

        internal static void ReferenceTypeArray()
        {
            PersonReferenceType[] array1 = PersonReferenceType.Random(100).ToArray();
            PersonReferenceType[] array2 = array1.ToArray(); // Copy.
            PersonReferenceType[] array3 = array1.ToArray(); // Copy.
            PersonReferenceType[] array4 = array1.ToArray(); // Copy.
            Trace.WriteLine($"{nameof(ArraySort)}: {array1.Run(ArraySort)}");
            Trace.WriteLine($"{nameof(LinqOrderBy)}: {array2.Run(LinqOrderBy)}");
            Trace.WriteLine($"{nameof(CustomLinqOrderBy)}: {array4.Run(CustomLinqOrderBy)}");
            Trace.WriteLine($"{nameof(FunctionalQuickSort)}: {array3.Run(FunctionalQuickSort)}");
        }
    }

    // Impure.
    internal static partial class Filter
    {
        internal static T[] EagerForEach<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            T[] result = new T[4];
            int count = 0;
            foreach (T value in source)
            {
                if (predicate(value))
                {
                    if (result.Length == count)
                    {
                        T[] newValues = new T[checked(count * 2)];
                        Array.Copy(result, 0, newValues, 0, count);
                        result = newValues;
                    }

                    result[count] = value;
                    count++;
                }
            }

            return result;
        }

        internal static IEnumerable<T> LazyForEach<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (T value in source)
            {
                if (predicate(value))
                {
                    yield return value;
                }
            }
        }

        internal static IEnumerable<T> Linq<T>
            (IEnumerable<T> source, Func<T, bool> predicate)
                => from value in source
                   where predicate(value)
                   select value;

        internal static IEnumerable<T> Monad<T>
            (IEnumerable<T> source, Func<T, bool> predicate)
                => from value in source
                   from result in predicate(value) ? Enumerable.Empty<T>() : value.Enumerable()
                   select result;
    }

    // Impure.
    internal static partial class Filter
    {
        internal static void Int32Sequence()
        {
            IEnumerable<int>[] arrays1 = ArrayHelper.RandomArrays(int.MinValue, int.MaxValue, 0, 100, 100);
            IEnumerable<int>[] arrays2 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            IEnumerable<int>[] arrays3 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            IEnumerable<int>[] arrays4 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            Func<int, bool> predicate = value => value > 0;
            Trace.WriteLine($"{nameof(Linq)}: {arrays1.RunEach(Linq, predicate)}");
            Trace.WriteLine($"{nameof(EagerForEach)}: {arrays2.RunEach(EagerForEach, predicate)}");
            Trace.WriteLine($"{nameof(LazyForEach)}: {arrays3.RunEach(LazyForEach, predicate)}");
            Trace.WriteLine($"{nameof(Monad)}: {arrays4.RunEach(Monad, predicate)}");
        }

        internal static void StringSequence()
        {
            IEnumerable<string> array1 = Enumerable.Range(0, 1000).Select(_ => Guid.NewGuid().ToString()).ToArray();
            IEnumerable<string> array2 = array1.ToArray(); // Copy.
            IEnumerable<string> array3 = array1.ToArray(); // Copy.
            IEnumerable<string> array4 = array1.ToArray(); // Copy.
            Func<string, bool> predicate = value => string.Compare(value, "x", StringComparison.OrdinalIgnoreCase) > 0;
            Trace.WriteLine($"{nameof(Linq)}: {array1.Run(Linq, predicate)}");
            Trace.WriteLine($"{nameof(EagerForEach)}: {array2.Run(EagerForEach, predicate)}");
            Trace.WriteLine($"{nameof(LazyForEach)}: {array3.Run(LazyForEach, predicate)}");
            Trace.WriteLine($"{nameof(Monad)}: {array4.Run(Monad, predicate)}");
        }

        internal static void ValueTypeSequence()
        {
            IEnumerable<PersonValueType> array1 = PersonValueType.Random(1000).ToArray();
            IEnumerable<PersonValueType> array2 = array1.ToArray(); // Copy.
            IEnumerable<PersonValueType> array3 = array1.ToArray(); // Copy.
            IEnumerable<PersonValueType> array4 = array1.ToArray(); // Copy.
            Func<PersonValueType, bool> predicate = value => value.Age > 18;
            Trace.WriteLine($"{nameof(Linq)}: {array1.Run(Linq, predicate)}");
            Trace.WriteLine($"{nameof(EagerForEach)}: {array2.Run(EagerForEach, predicate)}");
            Trace.WriteLine($"{nameof(LazyForEach)}: {array3.Run(LazyForEach, predicate)}");
            Trace.WriteLine($"{nameof(Monad)}: {array4.Run(Monad, predicate)}");
        }

        internal static void ReferenceTypeSequence()
        {
            IEnumerable<PersonReferenceType> array1 = PersonReferenceType.Random(1000).ToArray();
            IEnumerable<PersonReferenceType> array2 = array1.ToArray(); // Copy.
            IEnumerable<PersonReferenceType> array3 = array1.ToArray(); // Copy.
            IEnumerable<PersonReferenceType> array4 = array1.ToArray(); // Copy.
            Func<PersonReferenceType, bool> predicate = value => value.Age > 18;
            Trace.WriteLine($"{nameof(Linq)}: {array1.Run(Linq, predicate)}");
            Trace.WriteLine($"{nameof(EagerForEach)}: {array2.Run(EagerForEach, predicate)}");
            Trace.WriteLine($"{nameof(LazyForEach)}: {array3.Run(LazyForEach, predicate)}");
            Trace.WriteLine($"{nameof(Monad)}: {array4.Run(Monad, predicate)}");
        }
    }

    // Impure.
    internal static partial class Filter
    {
        internal static PersonReferenceType[] WithoutLambda(
            this PersonReferenceType[] source,
            int minAge1, int maxAge1, int minAge2, int maxAge2,
            string minName1, string maxName1, string minName2, string maxName2)
        {
            PersonReferenceType[] result = new PersonReferenceType[source.Length];
            int resultIndex = 0;
            foreach (PersonReferenceType person in source)
            {
                if ((person.Age >= minAge1 && person.Age <= maxAge2 || person.Age >= minAge2 && person.Age <= maxAge2)
                    && (string.Compare(person.Name, minName1, StringComparison.OrdinalIgnoreCase) >= 0
                        && string.Compare(person.Name, maxName1, StringComparison.OrdinalIgnoreCase) <= 0
                        || string.Compare(person.Name, minName2, StringComparison.OrdinalIgnoreCase) >= 0
                        && string.Compare(person.Name, maxName2, StringComparison.OrdinalIgnoreCase) <= 0))
                {
                    result[resultIndex++] = person;
                }
            }

            Array.Resize(ref result, resultIndex);
            return result;
        }

        internal static PersonReferenceType[] WithLambda(
            this PersonReferenceType[] source,
            int minAge1, int maxAge1, int minAge2, int maxAge2,
            string minName1, string maxName1, string minName2, string maxName2)
            => source.Where(person =>
                (person.Age >= minAge1 && person.Age <= maxAge2 || person.Age >= minAge2 && person.Age <= maxAge2)
                && (string.Compare(person.Name, minName1, StringComparison.OrdinalIgnoreCase) >= 0
                    && string.Compare(person.Name, maxName1, StringComparison.OrdinalIgnoreCase) <= 0
                    || string.Compare(person.Name, minName2, StringComparison.OrdinalIgnoreCase) >= 0
                    && string.Compare(person.Name, maxName2, StringComparison.OrdinalIgnoreCase) <= 0)).ToArray();
    }

    internal static partial class Filter
    {
        [CompilerGenerated]
        private sealed class Predicate
        {
            public int minAge1; public int minAge2; public int maxAge1; public int maxAge2;

            public string minName1; public string maxName1; public string minName2; public string maxName2;

            public bool WithLambda(PersonReferenceType person)
                => ((person.Age >= this.minAge1 && person.Age <= this.maxAge1)
                        || (person.Age >= this.minAge2 && person.Age <= this.maxAge2))
                    && ((string.Compare(person.Name, this.minName1, StringComparison.OrdinalIgnoreCase) >= 0
                            && string.Compare(person.Name, this.maxName1, StringComparison.OrdinalIgnoreCase) <= 0)
                        || (string.Compare(person.Name, this.minName2, StringComparison.OrdinalIgnoreCase) >= 0
                            && string.Compare(person.Name, this.maxName2, StringComparison.OrdinalIgnoreCase) <= 0));
        }

        internal static PersonReferenceType[] CompiledWithLambda(
            this PersonReferenceType[] source,
            int minAge1, int maxAge1, int minAge2, int maxAge2,
            string minName1, string maxName1, string minName2, string maxName2)
                => source.Where(new Predicate
                    {
                        minAge1 = minAge1, minAge2 = minAge2, maxAge1 = maxAge1, maxAge2 = maxAge2,
                        minName1 = minName1, maxName1 = maxName1, minName2 = minName2, maxName2 = maxName2
                    }.WithLambda).ToArray();
    }

    // Impure.
    internal static partial class Filter
    {
        internal static void ByPredicate()
        {
            PersonReferenceType[] array1 = PersonReferenceType.Random(10000).ToArray();
            PersonReferenceType[] array2 = array1.ToArray(); // Copy.
            string minName1 = Guid.NewGuid().ToString();
            string maxName1 = Guid.NewGuid().ToString();
            string minName2 = Guid.NewGuid().ToString();
            string maxName2 = Guid.NewGuid().ToString();
            Trace.WriteLine(
                $@"{nameof(WithoutLambda)}: {array1.Run(values =>
                    WithoutLambda(values, 10, 20, 30, 40, minName1, maxName1, minName2, maxName2))}");
            Trace.WriteLine(
                $@"{nameof(WithLambda)}: {array2.Run(values =>
                    WithLambda(values, 10, 20, 30, 40, minName1, maxName1, minName2, maxName2))}");
        }
    }
}
