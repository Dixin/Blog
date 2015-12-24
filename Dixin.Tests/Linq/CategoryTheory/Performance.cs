namespace Dixin.Linq.CategoryTheory.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public static class StopwatchExtensions
    {
        public static Stopwatch Run(this Stopwatch stopwatch, Action action, int count = 1)
        {
            stopwatch.Reset();
            if (count <= 0)
            {
                return stopwatch;
            }

            stopwatch.Start();
            for (int i = 0; i < count; i++)
            {
                action();
            }

            stopwatch.Stop();
            return stopwatch;
        }
    }

    public class PersonReferenceType : IComparable<PersonReferenceType>
    {
        private static readonly string LongString =
            Enumerable.Range(0, 10000).Select(_ => Guid.NewGuid().ToString()).Aggregate(string.Concat);

        public string Name { get; set; }

        public int Age { get; set; }

        public string Description { get; set; }

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

    public class PersonValueType : IComparable<PersonValueType>
    {
        private static readonly string LongString =
            Enumerable.Range(0, 10000).Select(_ => Guid.NewGuid().ToString()).Aggregate(string.Concat);

        public string Name { get; set; }

        public int Age { get; set; }

        public string Description { get; set; }

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

    public static partial class SortArrayTests
    {
        public static void ArraySort<T>(T[] array)
        {
            Array.Sort(array); // Impure. Sort items in place.
            array.ForEach(); // Iterate sorting result.
        }

        public static void LinqOrderBy<T>(T[] array)
        {
            array.OrderBy(value => value) // Pure. Create new array with sorted items.
                .ForEach(); // Iterate sorting result.
        }

        public static void FunctionalQuickSort<T>(T[] array)
        {
            array.QuickSort() // Pure. Create new array with sorted items.
                .ForEach(); // Iterate sorting result.
        }
    }

    public static partial class SortArrayTests
    {
        public static Tuple<long, long, long> SortInt32ArrayTest()
        {
            int[][] arrays1 = ArrayHelper.Random(int.MinValue, int.MaxValue, 0, 100, 100).ToArray();
            int[][] arrays2 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            int[][] arrays3 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            Stopwatch stopwatch = new Stopwatch();
            return new Tuple<long, long, long>(
                stopwatch.Run(() => arrays1.ForEach(ArraySort), 100).ElapsedMilliseconds,
                stopwatch.Run(() => arrays2.ForEach(LinqOrderBy), 100).ElapsedMilliseconds,
                stopwatch.Run(() => arrays3.ForEach(FunctionalQuickSort), 100).ElapsedMilliseconds);
        }

        public static Tuple<long, long, long> SortStringArrayTest()
        {
            string[] array1 = Enumerable.Range(0, 1000).Select(_ => Guid.NewGuid().ToString()).ToArray();
            string[] array2 = array1.ToArray(); // Copy.
            string[] array3 = array1.ToArray(); // Copy.
            Stopwatch stopwatch = new Stopwatch();
            return new Tuple<long, long, long>(
                stopwatch.Run(() => ArraySort(array1), 100).ElapsedMilliseconds,
                stopwatch.Run(() => LinqOrderBy(array2), 100).ElapsedMilliseconds,
                stopwatch.Run(() => FunctionalQuickSort(array3), 100).ElapsedMilliseconds);
        }

        public static Tuple<long, long, long> SortValueTypeArrayTest()
        {
            PersonValueType[] array1 = PersonValueType.Random(1000).ToArray();
            PersonValueType[] array2 = array1.ToArray(); // Copy.
            PersonValueType[] array3 = array1.ToArray(); // Copy.
            Stopwatch stopwatch = new Stopwatch();
            return new Tuple<long, long, long>(
                stopwatch.Run(() => ArraySort(array1), 100).ElapsedMilliseconds,
                stopwatch.Run(() => LinqOrderBy(array2), 100).ElapsedMilliseconds,
                stopwatch.Run(() => FunctionalQuickSort(array3), 100).ElapsedMilliseconds);
        }

        public static Tuple<long, long, long> SortReferenceTypeArrayTest()
        {
            PersonReferenceType[] array1 = PersonReferenceType.Random(100).ToArray();
            PersonReferenceType[] array2 = array1.ToArray(); // Copy.
            PersonReferenceType[] array3 = array1.ToArray(); // Copy.
            Stopwatch stopwatch = new Stopwatch();
            return new Tuple<long, long, long>(
                stopwatch.Run(() => ArraySort(array1), 1000).ElapsedMilliseconds,
                stopwatch.Run(() => LinqOrderBy(array2), 100).ElapsedMilliseconds,
                stopwatch.Run(() => FunctionalQuickSort(array3), 100).ElapsedMilliseconds);
        }

        public static void Print()
        {
            Tuple<long, long, long> int32Result = SortInt32ArrayTest();
            Trace.WriteLine(int32Result.Item1);
            Trace.WriteLine(int32Result.Item2);
            Trace.WriteLine(int32Result.Item3);
            Trace.WriteLine(string.Empty);

            Tuple<long, long, long> stringResult = SortStringArrayTest();
            Trace.WriteLine(stringResult.Item1);
            Trace.WriteLine(stringResult.Item2);
            Trace.WriteLine(stringResult.Item3);
            Trace.WriteLine(string.Empty);

            Tuple<long, long, long> valueTypeResult = SortValueTypeArrayTest();
            Trace.WriteLine(valueTypeResult.Item1);
            Trace.WriteLine(valueTypeResult.Item2);
            Trace.WriteLine(valueTypeResult.Item3);
            Trace.WriteLine(string.Empty);

            Tuple<long, long, long> referenceTypeResult = SortReferenceTypeArrayTest();
            Trace.WriteLine(referenceTypeResult.Item1);
            Trace.WriteLine(referenceTypeResult.Item2);
            Trace.WriteLine(referenceTypeResult.Item3);
            Trace.WriteLine(string.Empty);
        }
    }

    public static partial class FilterEnumerableTests
    {
        [Pure]
        public static T[] EagerForEach<T>(IEnumerable<T> source, Func<T, bool> predicate)
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

        [Pure]
        public static IEnumerable<T> ForEach<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (T value in source)
            {
                if (predicate(value))
                {
                    yield return value;
                }
            }
        }

        [Pure]
        public static IEnumerable<T> Linq<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            return from value in source
                   where predicate(value)
                   select value;
        }

        [Pure]
        public static IEnumerable<T> Monad<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            return from value in source
                   from result in predicate(value) ? Enumerable.Empty<T>() : value.Enumerable()
                   select result;
        }
    }

    public static partial class FilterEnumerableTests
    {
        public static Tuple<long, long, long, long> FilterInt32EnumerableTest()
        {
            IEnumerable<int>[] arrays1 = ArrayHelper.Random(int.MinValue, int.MaxValue, 0, 1000, 1000).ToArray();
            IEnumerable<int>[] arrays2 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            IEnumerable<int>[] arrays3 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            IEnumerable<int>[] arrays4 = arrays1.Select(array => array.ToArray()).ToArray(); // Copy.
            Stopwatch stopwatch = new Stopwatch();
            return new Tuple<long, long, long, long>(
                stopwatch.Run(() => arrays1.ForEach(array =>
                    EagerForEach(array, value => value > 0).ForEach()), 1000).ElapsedMilliseconds,
                stopwatch.Run(() => arrays2.ForEach(array =>
                    ForEach(array, value => value > 0).ForEach()), 1000).ElapsedMilliseconds,
                stopwatch.Run(() => arrays3.ForEach(array =>
                    Linq(array, value => value > 0).ForEach()), 1000).ElapsedMilliseconds,
                stopwatch.Run(() => arrays4.ForEach(array =>
                    Monad(array, value => value > 0).ForEach()), 1000).ElapsedMilliseconds);
        }

        public static Tuple<long, long, long, long> FilterStringEnumerableTest()
        {
            IEnumerable<string> array1 = Enumerable.Range(0, 10000).Select(_ => Guid.NewGuid().ToString()).ToArray();
            IEnumerable<string> array2 = array1.ToArray(); // Copy.
            IEnumerable<string> array3 = array1.ToArray(); // Copy.
            IEnumerable<string> array4 = array1.ToArray(); // Copy.
            Stopwatch stopwatch = new Stopwatch();
            return new Tuple<long, long, long, long>(
                stopwatch.Run(() => EagerForEach(array1, value => string.Compare(value, "x", StringComparison.OrdinalIgnoreCase) > 0).ForEach(), 10000).ElapsedMilliseconds,
                stopwatch.Run(() => ForEach(array2, value => string.Compare(value, "x", StringComparison.OrdinalIgnoreCase) > 0).ForEach(), 10000).ElapsedMilliseconds,
                stopwatch.Run(() => Linq(array3, value => string.Compare(value, "x", StringComparison.OrdinalIgnoreCase) > 0).ForEach(), 10000).ElapsedMilliseconds,
                stopwatch.Run(() => Monad(array4, value => string.Compare(value, "x", StringComparison.OrdinalIgnoreCase) > 0).ForEach(), 10000).ElapsedMilliseconds);
        }

        public static Tuple<long, long, long, long> FilterValueTypeEnumerableTest()
        {
            IEnumerable<PersonValueType> array1 = PersonValueType.Random(10000).ToArray();
            IEnumerable<PersonValueType> array2 = array1.ToArray(); // Copy.
            IEnumerable<PersonValueType> array3 = array1.ToArray(); // Copy.
            IEnumerable<PersonValueType> array4 = array1.ToArray(); // Copy.
            Stopwatch stopwatch = new Stopwatch();
            return new Tuple<long, long, long, long>(
                stopwatch.Run(() => EagerForEach(array1, value => value.Age > 50).ForEach(), 10000).ElapsedMilliseconds,
                stopwatch.Run(() => ForEach(array2, value => value.Age > 50).ForEach(), 10000).ElapsedMilliseconds,
                stopwatch.Run(() => Linq(array3, value => value.Age > 50).ForEach(), 10000).ElapsedMilliseconds,
                stopwatch.Run(() => Monad(array4, value => value.Age > 50).ForEach(), 10000).ElapsedMilliseconds);
        }

        public static Tuple<long, long, long, long> FilterReferenceTypeEnumerableTest()
        {
            IEnumerable<PersonReferenceType> array1 = PersonReferenceType.Random(1000).ToArray();
            IEnumerable<PersonReferenceType> array2 = array1.ToArray(); // Copy.
            IEnumerable<PersonReferenceType> array3 = array1.ToArray(); // Copy.
            IEnumerable<PersonReferenceType> array4 = array1.ToArray(); // Copy.
            Stopwatch stopwatch = new Stopwatch();
            return new Tuple<long, long, long, long>(
                stopwatch.Run(() => EagerForEach(array1, value => value.Age > 50).ForEach(), 10000).ElapsedMilliseconds,
                stopwatch.Run(() => ForEach(array2, value => value.Age > 50).ForEach(), 10000).ElapsedMilliseconds,
                stopwatch.Run(() => Linq(array3, value => value.Age > 50).ForEach(), 10000).ElapsedMilliseconds,
                stopwatch.Run(() => Monad(array4, value => value.Age > 50).ForEach(), 10000).ElapsedMilliseconds);
        }

        public static void Print()
        {
            Tuple<long, long, long, long> int32Result = FilterInt32EnumerableTest();
            Trace.WriteLine(int32Result.Item1);
            Trace.WriteLine(int32Result.Item2);
            Trace.WriteLine(int32Result.Item3);
            Trace.WriteLine(int32Result.Item4);
            Trace.WriteLine(string.Empty);

            Tuple<long, long, long, long> stringResult = FilterStringEnumerableTest();
            Trace.WriteLine(stringResult.Item1);
            Trace.WriteLine(stringResult.Item2);
            Trace.WriteLine(stringResult.Item3);
            Trace.WriteLine(stringResult.Item4);
            Trace.WriteLine(string.Empty);

            Tuple<long, long, long, long> valueTypeResult = FilterValueTypeEnumerableTest();
            Trace.WriteLine(valueTypeResult.Item1);
            Trace.WriteLine(valueTypeResult.Item2);
            Trace.WriteLine(valueTypeResult.Item3);
            Trace.WriteLine(valueTypeResult.Item4);
            Trace.WriteLine(string.Empty);

            Tuple<long, long, long, long> referenceTypeResult = FilterReferenceTypeEnumerableTest();
            Trace.WriteLine(referenceTypeResult.Item1);
            Trace.WriteLine(referenceTypeResult.Item2);
            Trace.WriteLine(referenceTypeResult.Item3);
            Trace.WriteLine(referenceTypeResult.Item4);
            Trace.WriteLine(string.Empty);
        }
    }

    public static partial class FillterArrayTests
    {
        public static PersonReferenceType[] WithoutLambda(
            this PersonReferenceType[] source,
            int minAge1, int maxAge1,
            int minAge2, int maxAge2,
            string minName1, string maxName1,
            string minName2, string maxName2)
        {
            PersonReferenceType[] result = new PersonReferenceType[source.Length];
            int resultIndex = 0;
            foreach (PersonReferenceType person in source)
            {
                if ((person.Age >= minAge1 && person.Age <= maxAge2 || person.Age >= minAge2 && person.Age <= maxAge2) &&
                    (string.Compare(person.Name, minName1, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    string.Compare(person.Name, maxName1, StringComparison.OrdinalIgnoreCase) <= 0 ||
                    string.Compare(person.Name, minName2, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    string.Compare(person.Name, maxName2, StringComparison.OrdinalIgnoreCase) <= 0))
                {
                    result[resultIndex++] = person;
                }
            }

            Array.Resize(ref result, resultIndex);
            return result;
        }

        public static PersonReferenceType[] Lambda(
            this PersonReferenceType[] source,
            int minAge1, int maxAge1,
            int minAge2, int maxAge2,
            string minName1, string maxName1,
            string minName2, string maxName2)
        {
            return source
                .Where(person =>
                    (person.Age >= minAge1 && person.Age <= maxAge2 || person.Age >= minAge2 && person.Age <= maxAge2) &&
                    (string.Compare(person.Name, minName1, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    string.Compare(person.Name, maxName1, StringComparison.OrdinalIgnoreCase) <= 0 ||
                    string.Compare(person.Name, minName2, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    string.Compare(person.Name, maxName2, StringComparison.OrdinalIgnoreCase) <= 0))
                .ToArray();
        }

        [CompilerGenerated]
        private sealed class Predicate
        {
            public int minAge1;

            public int minAge2;

            public int maxAge2;

            public string minName1;

            public string maxName1;

            public string minName2;

            public string maxName2;

            public bool Lambda(PersonReferenceType person)
            {
                return ((person.Age >= this.minAge1 && person.Age <= this.maxAge2) || (person.Age >= this.minAge2 && person.Age <= this.maxAge2)) && ((string.Compare(person.Name, this.minName1, StringComparison.OrdinalIgnoreCase) >= 0 && string.Compare(person.Name, this.maxName1, StringComparison.OrdinalIgnoreCase) <= 0) || (string.Compare(person.Name, this.minName2, StringComparison.OrdinalIgnoreCase) >= 0 && string.Compare(person.Name, this.maxName2, StringComparison.OrdinalIgnoreCase) <= 0));
            }
        }

        public static PersonReferenceType[] CompiledLambda(this PersonReferenceType[] source, int minAge1, int maxAge1, int minAge2, int maxAge2, string minName1, string maxName1, string minName2, string maxName2)
        {
            Predicate predicate = new Predicate();
            predicate.minAge1 = minAge1;
            predicate.minAge2 = minAge2;
            predicate.maxAge2 = maxAge2;
            predicate.minName1 = minName1;
            predicate.maxName1 = maxName1;
            predicate.minName2 = minName2;
            predicate.maxName2 = maxName2;

            return source.Where(predicate.Lambda).ToArray();
        }
    }
    public static partial class FillterArrayTests
    {
        public static Tuple<long, long> FilterArrayTest()
        {
            PersonReferenceType[] array1 = PersonReferenceType.Random(1000).ToArray();
            PersonReferenceType[] array2 = array1.ToArray(); // Copy.
            string minName1 = Guid.NewGuid().ToString();
            string maxName1 = Guid.NewGuid().ToString();
            string minName2 = Guid.NewGuid().ToString();
            string maxName2 = Guid.NewGuid().ToString();

            Stopwatch stopwatch = new Stopwatch();
            return new Tuple<long, long>(
                stopwatch.Run(() => WithoutLambda(array1, 10, 20, 30, 40, minName1, maxName1, minName2, maxName2), 10000).ElapsedMilliseconds,
                stopwatch.Run(() => Lambda(array2, 10, 20, 30, 40, minName1, maxName1, minName2, maxName2), 10000).ElapsedMilliseconds);
        }

        private static void Main()
        {
            //Tuple<long, long> int32Result = FilterArrayTest();
            //Trace.WriteLine(int32Result.Item1);
            //Trace.WriteLine(int32Result.Item2);

            //FilterEnumerableTests.Print();
        }
    }



    public static class ArrayHelper
    {
        public static IEnumerable<int[]> Random(int minValue, int maxValue, int minLength, int maxLength, int count)
        {
            for (int index = 0; index < count; index++)
            {
                yield return Random(minValue, maxValue, minLength, maxLength);
            }
        }

        public static int[] Random(int minValue, int maxValue, int minLength, int maxLength)
        {
            minLength = minLength < 0 ? 0 : minLength;
            maxLength = maxLength < minLength ? minLength : maxLength;
            Random random = new Random();
            int arrayLength = random.Next(minLength, maxLength);
            int[] array = new int[arrayLength];
            for (int arrayIndex = 0; arrayIndex < arrayLength; arrayIndex++)
            {
                array[arrayIndex] = random.Next(minValue, maxValue);
            }

            return array;
        }
    }

}

