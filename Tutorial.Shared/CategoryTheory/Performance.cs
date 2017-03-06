namespace Tutorial.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using CustomLinq = Tutorial.LinqToObjects.EnumerableExtensions;
    using EnumerableX = Tutorial.LinqToObjects.EnumerableX;

    using static Tutorial.LinqToObjects.EnumerableX;

    public static partial class EnumerableExtensions
    {
        internal static IEnumerable<T> PureQuickSort<T>(this IEnumerable<T> source, Comparer<T> comparer = null)
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
                 select value).PureQuickSort(); // Recursion.
            IEnumerable<T> greaterThanHead =
                (from value in tail
                 where comparer.Compare(value, head) > 0
                 select value).PureQuickSort(); // Recursion.
            return smallerThanHead.Concat(head.Enumerable()).Concat(greaterThanHead);
        }

        internal static IEnumerable<T> QuickSort<T>(this IEnumerable<T> source, Comparer<T> comparer = null)
        {
            using (IEnumerator<T> iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    yield break; // Recursion terminates.
                }

                T first = iterator.Current;
                comparer = comparer ?? Comparer<T>.Default;
                List<T> smallerThanOrEqualToFIrst = new List<T>();
                List<T> greaterThanFirst = new List<T>();
                while (iterator.MoveNext())
                {
                    T value = iterator.Current;
                    if (comparer.Compare(value, first) <= 0)
                    {
                        smallerThanOrEqualToFIrst.Add(value);
                    }
                    else
                    {
                        greaterThanFirst.Add(value);
                    }
                } // Eager evaluation.

                foreach (T value in smallerThanOrEqualToFIrst.QuickSort()) // Recursion.
                {
                    yield return value;
                }
                yield return first;
                foreach (T value in greaterThanFirst.QuickSort()) // Recursion.
                {
                    yield return value;
                }
            }
        }
    }

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

        public static long RunEach<T>(
            this IEnumerable<T> args, Func<T, T> action, int count = DefaultCount, Stopwatch stopwatch = null) =>
                Run(() => args.ForEach(arg => action(arg)), count);

        public static long RunEach<T1, T2>(
            this IEnumerable<IEnumerable<T1>> args1,
            Func<IEnumerable<T1>, Func<T1, T2>, IEnumerable<T1>> action,
            Func<T1, T2> arg2,
            int count = DefaultCount,
            Stopwatch stopwatch = null) => 
                Run(() => args1.ForEach(arg1 => action(arg1, arg2).ForEach()), count);

        public static long Run<T>(
            this T arg, Func<T, T> action, int count = DefaultCount, Stopwatch stopwatch = null) =>
                Run(() => action(arg), count);

        public static long Run<T1, T2>(
            this IEnumerable<T1> arg1,
            Func<IEnumerable<T1>, Func<T1, T2>, IEnumerable<T1>> action,
            Func<T1, T2> arg2,
            int count = DefaultCount,
            Stopwatch stopwatch = null) => 
                Run(() => action(arg1, arg2).ForEach(), count);
    }

    public static class ArrayHelper
    {
        public static int[][] RandomArrays(int minValue, int maxValue, int minLength, int maxLength, int count) => 
            Enumerable
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

    internal class Person : IComparable<Person>
    {
        private static readonly string LongString =
            Enumerable.Range(0, 10_000).Select(_ => Guid.NewGuid().ToString()).Aggregate(string.Concat);

        internal string Name { get; private set; }

        internal int Age { get; private set; }

        internal string Description { get; private set; }

        internal static IEnumerable<Person> Random(int count)
        {
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                yield return new Person()
                {
                    Name = Guid.NewGuid().ToString(),
                    Age = random.Next(0, 100),
                    Description = LongString
                };
            }
        }

        public int CompareTo(Person other)
        {
            int nameCompare = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
            return nameCompare != 0 ? nameCompare : this.Age.CompareTo(other.Age);
        }
    }

    internal struct ValuePerson : IComparable<ValuePerson>
    {
        private static readonly string LongString =
            Enumerable.Range(0, 10_000).Select(_ => Guid.NewGuid().ToString()).Aggregate(string.Concat);

        internal string Name { get; private set; }

        internal int Age { get; private set; }

        internal string Description { get; private set; }

        internal static IEnumerable<ValuePerson> Random(int count)
        {
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                yield return new ValuePerson()
                {
                    Name = Guid.NewGuid().ToString(),
                    Age = random.Next(0, 100),
                    Description = LongString
                };
            }
        }

        public int CompareTo(ValuePerson other)
        {
            int nameCompare = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
            return nameCompare != 0 ? nameCompare : this.Age.CompareTo(other.Age);
        }
    }

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

        internal static T[] PureQuickSort<T>(T[] array) => array.PureQuickSort().ToArray();
    }

    internal static partial class Sort
    {
        internal static void Int32Array()
        {
            int[][] arrays1 = ArrayHelper.RandomArrays(int.MinValue, int.MaxValue, 0, 100, 100);
            int[][] arrays2 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            int[][] arrays3 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            int[][] arrays4 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            int[][] arrays5 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            $"{nameof(ArraySort)}: {arrays1.RunEach(ArraySort)}".WriteLine();
            $"{nameof(LinqOrderBy)}: {arrays2.RunEach(LinqOrderBy)}".WriteLine();
            $"{nameof(CustomLinqOrderBy)}: {arrays3.RunEach(CustomLinqOrderBy)}".WriteLine();
            $"{nameof(FunctionalQuickSort)}: {arrays4.RunEach(FunctionalQuickSort)}".WriteLine();
            $"{nameof(PureQuickSort)}: {arrays5.RunEach(PureQuickSort)}".WriteLine();
        }

        internal static void StringArray()
        {
            string[] array1 = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid().ToString()).ToArray();
            string[] array2 = array1.ToArray(); // Copy.
            string[] array3 = array1.ToArray(); // Copy.
            string[] array4 = array1.ToArray(); // Copy.
            string[] array5 = array1.ToArray(); // Copy.
            $"{nameof(ArraySort)}: {array1.Run(ArraySort)}".WriteLine();
            $"{nameof(LinqOrderBy)}: {array2.Run(LinqOrderBy)}".WriteLine();
            $"{nameof(CustomLinqOrderBy)}: {array3.Run(CustomLinqOrderBy)}".WriteLine();
            $"{nameof(FunctionalQuickSort)}: {array4.Run(FunctionalQuickSort)}".WriteLine();
            $"{nameof(PureQuickSort)}: {array5.Run(PureQuickSort)}".WriteLine();
        }

        internal static void ValueTypeArray()
        {
            ValuePerson[] array1 = ValuePerson.Random(100).ToArray();
            ValuePerson[] array2 = array1.ToArray(); // Copy.
            ValuePerson[] array3 = array1.ToArray(); // Copy.
            ValuePerson[] array4 = array1.ToArray(); // Copy.
            ValuePerson[] array5 = array1.ToArray(); // Copy.
            $"{nameof(ArraySort)}: {array1.Run(ArraySort)}".WriteLine();
            $"{nameof(LinqOrderBy)}: {array2.Run(LinqOrderBy)}".WriteLine();
            $"{nameof(CustomLinqOrderBy)}: {array4.Run(CustomLinqOrderBy)}".WriteLine();
            $"{nameof(FunctionalQuickSort)}: {array3.Run(FunctionalQuickSort)}".WriteLine();
            $"{nameof(PureQuickSort)}: {array5.Run(PureQuickSort)}".WriteLine();
        }

        internal static void ReferenceTypeArray()
        {
            Person[] array1 = Person.Random(100).ToArray();
            Person[] array2 = array1.ToArray(); // Copy.
            Person[] array3 = array1.ToArray(); // Copy.
            Person[] array4 = array1.ToArray(); // Copy.
            Person[] array5 = array1.ToArray(); // Copy.
            $"{nameof(ArraySort)}: {array1.Run(ArraySort)}".WriteLine();
            $"{nameof(LinqOrderBy)}: {array2.Run(LinqOrderBy)}".WriteLine();
            $"{nameof(CustomLinqOrderBy)}: {array4.Run(CustomLinqOrderBy)}".WriteLine();
            $"{nameof(FunctionalQuickSort)}: {array3.Run(FunctionalQuickSort)}".WriteLine();
            $"{nameof(PureQuickSort)}: {array5.Run(PureQuickSort)}".WriteLine();
        }
    }

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

    internal static partial class Filter
    {
        internal static void Int32Sequence()
        {
            IEnumerable<int>[] arrays1 = ArrayHelper.RandomArrays(int.MinValue, int.MaxValue, 0, 100, 100);
            IEnumerable<int>[] arrays2 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            IEnumerable<int>[] arrays3 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            IEnumerable<int>[] arrays4 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            Func<int, bool> predicate = value => value > 0;
            $"{nameof(Linq)}: {arrays1.RunEach(Linq, predicate)}".WriteLine();
            $"{nameof(EagerForEach)}: {arrays2.RunEach(EagerForEach, predicate)}".WriteLine();
            $"{nameof(LazyForEach)}: {arrays3.RunEach(LazyForEach, predicate)}".WriteLine();
            $"{nameof(Monad)}: {arrays4.RunEach(Monad, predicate)}".WriteLine();
        }

        internal static void StringSequence()
        {
            IEnumerable<string> array1 = Enumerable.Range(0, 1_000).Select(_ => Guid.NewGuid().ToString()).ToArray();
            IEnumerable<string> array2 = array1.ToArray(); // Copy.
            IEnumerable<string> array3 = array1.ToArray(); // Copy.
            IEnumerable<string> array4 = array1.ToArray(); // Copy.
            Func<string, bool> predicate = value => string.Compare(value, "x", StringComparison.OrdinalIgnoreCase) > 0;
            $"{nameof(Linq)}: {array1.Run(Linq, predicate)}".WriteLine();
            $"{nameof(EagerForEach)}: {array2.Run(EagerForEach, predicate)}".WriteLine();
            $"{nameof(LazyForEach)}: {array3.Run(LazyForEach, predicate)}".WriteLine();
            $"{nameof(Monad)}: {array4.Run(Monad, predicate)}".WriteLine();
        }

        internal static void ValueTypeSequence()
        {
            IEnumerable<ValuePerson> array1 = ValuePerson.Random(1_000).ToArray();
            IEnumerable<ValuePerson> array2 = array1.ToArray(); // Copy.
            IEnumerable<ValuePerson> array3 = array1.ToArray(); // Copy.
            IEnumerable<ValuePerson> array4 = array1.ToArray(); // Copy.
            Func<ValuePerson, bool> predicate = value => value.Age > 18;
            $"{nameof(Linq)}: {array1.Run(Linq, predicate)}".WriteLine();
            $"{nameof(EagerForEach)}: {array2.Run(EagerForEach, predicate)}".WriteLine();
            $"{nameof(LazyForEach)}: {array3.Run(LazyForEach, predicate)}".WriteLine();
            $"{nameof(Monad)}: {array4.Run(Monad, predicate)}".WriteLine();
        }

        internal static void ReferenceTypeSequence()
        {
            IEnumerable<Person> array1 = Person.Random(1_000).ToArray();
            IEnumerable<Person> array2 = array1.ToArray(); // Copy.
            IEnumerable<Person> array3 = array1.ToArray(); // Copy.
            IEnumerable<Person> array4 = array1.ToArray(); // Copy.
            Func<Person, bool> predicate = value => value.Age > 18;
            $"{nameof(Linq)}: {array1.Run(Linq, predicate)}".WriteLine();
            $"{nameof(EagerForEach)}: {array2.Run(EagerForEach, predicate)}".WriteLine();
            $"{nameof(LazyForEach)}: {array3.Run(LazyForEach, predicate)}".WriteLine();
            $"{nameof(Monad)}: {array4.Run(Monad, predicate)}".WriteLine();
        }
    }

    internal static partial class Filter
    {
        internal static Person[] WithoutLambda(
            this Person[] source,
            int minAge1, int maxAge1, int minAge2, int maxAge2,
            string minName1, string maxName1, string minName2, string maxName2)
        {
            Person[] result = new Person[source.Length];
            int resultIndex = 0;
            foreach (Person person in source)
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

        internal static Person[] WithLambda(
            this Person[] source,
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

            public bool WithLambda(Person person)
                => ((person.Age >= this.minAge1 && person.Age <= this.maxAge1)
                        || (person.Age >= this.minAge2 && person.Age <= this.maxAge2))
                    && ((string.Compare(person.Name, this.minName1, StringComparison.OrdinalIgnoreCase) >= 0
                            && string.Compare(person.Name, this.maxName1, StringComparison.OrdinalIgnoreCase) <= 0)
                        || (string.Compare(person.Name, this.minName2, StringComparison.OrdinalIgnoreCase) >= 0
                            && string.Compare(person.Name, this.maxName2, StringComparison.OrdinalIgnoreCase) <= 0));
        }

        internal static Person[] CompiledWithLambda(
            this Person[] source,
            int minAge1, int maxAge1, int minAge2, int maxAge2,
            string minName1, string maxName1, string minName2, string maxName2)
                => source.Where(new Predicate
                    {
                        minAge1 = minAge1, minAge2 = minAge2, maxAge1 = maxAge1, maxAge2 = maxAge2,
                        minName1 = minName1, maxName1 = maxName1, minName2 = minName2, maxName2 = maxName2
                    }.WithLambda).ToArray();
    }

    internal static partial class Filter
    {
        internal static void ByPredicate()
        {
            Person[] array1 = Person.Random(1_0000).ToArray();
            Person[] array2 = array1.ToArray(); // Copy.
            string minName1 = Guid.NewGuid().ToString();
            string maxName1 = Guid.NewGuid().ToString();
            string minName2 = Guid.NewGuid().ToString();
            string maxName2 = Guid.NewGuid().ToString();
            
                $@"{nameof(WithoutLambda)}: {array1.Run(values =>
                    WithoutLambda(values, 10, 20, 30, 40, minName1, maxName1, minName2, maxName2))}".WriteLine();
            
                $@"{nameof(WithLambda)}: {array2.Run(values =>
                    WithLambda(values, 10, 20, 30, 40, minName1, maxName1, minName2, maxName2))}".WriteLine();
        }
    }
}

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
