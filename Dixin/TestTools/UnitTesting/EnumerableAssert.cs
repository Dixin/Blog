namespace Dixin.TestTools.UnitTesting
{
    using System.Collections.Generic;

    using Dixin.Linq;
    using Dixin.Linq.LinqToObjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static partial class EnumerableAssert
    {
        public static void AreEqual<T>(
            IEnumerable<T> expected,
            IEnumerable<T> actual,
            IEqualityComparer<T> comparer = null,
            string message = null,
            params object[] parameters)
        {
            if (expected == null && actual == null)
            {
                return;
            }

            message = string.IsNullOrEmpty(message) ? string.Empty : $"{message} ";
            if (expected == null)
            {
                Assert.IsNull(
                    actual,
                    $"{message}Expected sequence is null, but actual sequence is not null.",
                    parameters);
                return;
            }

            Assert.IsNotNull(
                actual,
                $"{message}Expected sequence is not null, but actual sequence is null.",
                parameters);

            comparer = comparer ?? EqualityComparer<T>.Default;
            using (IEnumerator<T> expectedItorator = expected.GetEnumerator())
            using (IEnumerator<T> actualIterator = actual.GetEnumerator())
            {
                int expectedIndex = 0;
                for (; expectedItorator.MoveNext(); expectedIndex++)
                {
                    Assert.IsTrue(
                        actualIterator.MoveNext(),
                        $"{message}Expected sequence has more than {expectedIndex} value(s), but actual sequence has {expectedIndex} value(s).",
                        parameters);

                    T expectedValue = expectedItorator.Current;
                    T actualValue = actualIterator.Current;
                    Assert.IsTrue(
                        comparer.Equals(expectedValue, actualValue),
                        $"{message}Expected and actual sequences' values are not equal at index {expectedIndex}. Expected value is {expectedValue}, but actual value is {actualValue}.",
                        parameters);
                }

                Assert.IsFalse(
                    actualIterator.MoveNext(),
                    $"{message}Expected sequence has {expectedIndex} value(s), but actual sequence has more than {expectedIndex} value(s).",
                    parameters);
            }
        }
    }

    public static partial class EnumerableAssert
    {
        public static void IsEmpty<T>(IEnumerable<T> actual, string message = null, params object[] parameters)
        {
            Assert.IsNotNull(actual, message, parameters);
            Assert.IsTrue(actual.IsEmpty(), message, parameters);
        }

        public static void IsNullOrEmpty<T>
            (IEnumerable<T> actual, string message = null, params object[] parameters) =>
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
}
