namespace Linq.Range.Tests
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AsEnumerableTests
    {
        [TestMethod]
        public void AsEnumerableTest()
        {
            int[] source = Enumerable.Range(0, 100).ToArray();

            // int count = 2147483638;

            // Multiple elements in the middle.
            Assert.IsTrue((^10..^0).AsEnumerable().ToArray().SequenceEqual(Enumerable.Range(2147483638, 10)));
            Assert.IsTrue((^2147483600..100).AsEnumerable().ToArray().SequenceEqual(source[48..100]));
            Assert.IsTrue((2..7).AsEnumerable().ToArray().SequenceEqual(source[2..7]));
            Assert.IsTrue((2..^2147483600).AsEnumerable().ToArray().SequenceEqual(source[2..48]));
            Assert.IsTrue((^2147483610..^2147483600).AsEnumerable().ToArray().SequenceEqual(source[38..48]));

            // Rang with one index.
            Assert.IsTrue((^48..).AsEnumerable().ToArray().SequenceEqual(Enumerable.Range(2147483600, 48)));
            Assert.IsTrue((2147483600..).AsEnumerable().ToArray().SequenceEqual(Enumerable.Range(2147483600, 48)));
            Assert.IsTrue((..^2147483600).AsEnumerable().ToArray().SequenceEqual(Enumerable.Range(0, 48)));
            Assert.IsTrue((..48).AsEnumerable().ToArray().SequenceEqual(Enumerable.Range(0, 48)));

            // All.
            Assert.IsTrue((..).AsEnumerable().Take(100).ToArray().SequenceEqual(source[..]));

            // Single element in the middle.
            Assert.IsTrue((^48..2147483601).AsEnumerable().ToArray().SequenceEqual(new int[] { 2147483600 }));
            Assert.IsTrue((48..49).AsEnumerable().ToArray().SequenceEqual(new int[] { 48 }));
            Assert.IsTrue((47..^2147483600).AsEnumerable().ToArray().SequenceEqual(new int[] { 47 }));
            Assert.IsTrue((^2147483601..^2147483600).AsEnumerable().ToArray().SequenceEqual(new int[] { 47 }));

            // Single element at start.
            // Assert.IsTrue((^2147483648..1).AsEnumerable().ToArray().SequenceEqual(new int[] { 0 }));
            Assert.IsTrue((0..1).AsEnumerable().ToArray().SequenceEqual(new int[] { 0 }));
            Assert.IsTrue((0..^2147483647).AsEnumerable().ToArray().SequenceEqual(new int[] { 0 }));
            // Assert.IsTrue((^2147483648..^2147483647).AsEnumerable().ToArray().SequenceEqual(new int[] { 0 }));

            // Single element at end.
            // Assert.IsTrue((^1..2147483648).AsEnumerable().ToArray().SequenceEqual(new int[] { 2147483647 }));
            // Assert.IsTrue((2147483647..2147483648).AsEnumerable().ToArray().SequenceEqual(new int[] { 2147483647 }));
            Assert.IsTrue((2147483647..^0).AsEnumerable().ToArray().SequenceEqual(new int[] { 2147483647 }));
            Assert.IsTrue((^1..^0).AsEnumerable().ToArray().SequenceEqual(new int[] { 2147483647 }));

            // No element.
            Assert.IsFalse((3..3).AsEnumerable().Any());
            Assert.IsFalse((48..^2147483600).AsEnumerable().Any());
            Assert.IsFalse((^2147483600..48).AsEnumerable().Any());
            Assert.IsFalse((^6..^6).AsEnumerable().Any());

            // Invalid range.
            Assert.ThrowsException<OverflowException>(() => (3..2).AsEnumerable().ToArray());
            Assert.ThrowsException<OverflowException>(() => (50..^2147483600).AsEnumerable().ToArray());
            Assert.ThrowsException<OverflowException>(() => (^2147483600..8).AsEnumerable().ToArray());
            Assert.ThrowsException<OverflowException>(() => (^6..^7).AsEnumerable().ToArray());
        }
    }
}
