namespace Tutorial.LinqToObjects
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

#if DEMO
    public static partial class EnumerableAssert
    {
        public static void IsNullOrEmpty<T>
            (IEnumerable<T> actual, string message = null, params object[] parameters)
        {
            using (IEnumerator<T> iterator = actual?.GetEnumerator())
            {
                Assert.IsTrue(iterator?.MoveNext() ?? false, message, parameters);
            }
        }

        public static void IsEmpty<T>(IEnumerable<T> actual, string message = null, params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            using (IEnumerator<T> iterator = actual.GetEnumerator())
            {
                Assert.IsFalse(iterator.MoveNext(), message, parameters);
            }
        }

        public static void Any<T>(IEnumerable<T> actual, string message = null, params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            using (IEnumerator<T> iterator = actual.GetEnumerator())
            {
                Assert.IsTrue(iterator.MoveNext(), message, parameters);
            }
        }
    }

    public static partial class EnumerableAssert
    {
        public static void Single<T>(IEnumerable<T> actual, string message = null, params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            using (IEnumerator<T> iterator = actual.GetEnumerator())
            {
                Assert.IsTrue(iterator.MoveNext() && !iterator.MoveNext(), message, parameters);
            }
        }

        public static void Multiple<T>(IEnumerable<T> actual, string message = null, params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            using (IEnumerator<T> iterator = actual.GetEnumerator())
            {
                Assert.IsTrue(iterator.MoveNext() && iterator.MoveNext(), message, parameters);
            }
        }
    }

    public static partial class EnumerableAssert
    {
        public static void Contains<T>(
            T expected,
            IEnumerable<T> actual,
            IEqualityComparer<T> comparer = null,
            string message = null,
            params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            comparer = comparer ?? EqualityComparer<T>.Default;
            foreach (T value in actual)
            {
                if (comparer.Equals(expected, value))
                {
                    return;
                }
            }
            Assert.Fail(message, parameters);
        }

        public static void DoesNotContain<T>(
            T expected, IEnumerable<T> actual, 
            IEqualityComparer<T> comparer = null,
            string message = null,
            params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            comparer = comparer ?? EqualityComparer<T>.Default;
            foreach (T value in actual)
            {
                if (comparer.Equals(expected, value))
                {
                    Assert.Fail(message, parameters);
                }
            }
        }
    }
#endif

    public static partial class EnumerableAssert
    {
        public static void IsEmpty<T>(IEnumerable<T> actual, string message = null, params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            Assert.IsTrue(actual.IsEmpty(), message, parameters);
        }

        public static void IsNullOrEmpty<T>(
            IEnumerable<T> actual, string message = null, params object[] parameters) =>
                Assert.IsTrue(actual.IsNullOrEmpty(), message, parameters);

        public static void Any<T>(IEnumerable<T> actual, string message = null, params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            Assert.IsTrue(actual.Any(), message, parameters);
        }

        public static void Single<T>(IEnumerable<T> actual, string message = null, params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            Assert.AreEqual(1, actual.Count(), message, parameters);
        }

        public static void Multiple<T>(IEnumerable<T> actual, string message = null, params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            using (IEnumerator<T> iterator = actual.GetEnumerator())
            {
                Assert.IsTrue(iterator.MoveNext() && iterator.MoveNext(), message, parameters);
            }
        }

        public static void Contains<T>(
            T expected,
            IEnumerable<T> actual,
            IEqualityComparer<T> comparer = null,
            string message = null,
            params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            Assert.IsTrue(actual.Contains(expected, comparer ?? EqualityComparer<T>.Default), message, parameters);
        }

        public static void DoesNotContain<T>(
            T expected,
            IEnumerable<T> actual,
            IEqualityComparer<T> comparer = null,
            string message = null,
            params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            Assert.IsFalse(actual.Contains(expected, comparer ?? EqualityComparer<T>.Default), message, parameters);
        }

        public static void Count<T>(
            int expected, IEnumerable<T> actual, string message = null, params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            Assert.AreEqual(expected, actual.Count(), message, parameters);
        }
    }

    public static partial class EnumerableAssert
    {
        public static void AreSequentialEqual<T>(
            IEnumerable<T> expected,
            IEnumerable<T> actual,
            IEqualityComparer<T> comparer = null,
            string message = null,
            params object[] parameters)
        {
            Assert.IsNotNull(expected, $"Expected sequence is null. {message}", parameters);
            Assert.IsNotNull(actual, $"Actual sequence is null. {message}", parameters);

            comparer = comparer ?? EqualityComparer<T>.Default;
            using (IEnumerator<T> expectedItorator = expected.GetEnumerator())
            using (IEnumerator<T> actualIterator = actual.GetEnumerator())
            {
                int expectedIndex = 0;
                for (; expectedItorator.MoveNext(); expectedIndex++)
                {
                    Assert.IsTrue(
                        actualIterator.MoveNext(),
                        $"Expected sequence has more than {expectedIndex} value(s), but actual sequence has {expectedIndex} value(s). {message}",
                        parameters);
                    T expectedValue = expectedItorator.Current;
                    T actualValue = actualIterator.Current;
                    Assert.IsTrue(
                        comparer.Equals(expectedValue, actualValue),
                        $"Expected and actual sequences' values are not equal at index {expectedIndex}. Expected value is {expectedValue}, but actual value is {actualValue}. {message}",
                        parameters);
                }
                Assert.IsFalse(
                    actualIterator.MoveNext(),
                    $"Expected sequence has {expectedIndex} value(s), but actual sequence has more than {expectedIndex} value(s). {message}",
                    parameters);
            }
        }
    }
}
