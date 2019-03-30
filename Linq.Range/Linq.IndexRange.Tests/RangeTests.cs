namespace Linq.Range.Tests
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static System.Linq.EnumerableExtensions;

    [TestClass]
    public class RangeTests
    {
        [TestMethod]
        public void RangeTest()
        {
            int[] source = Enumerable.Range(0, 100).ToArray();

            // int count = 2147483638;

            // Multiple elements in the middle.
            Assert.IsTrue(Range(^10..^0).ToArray().SequenceEqual(Enumerable.Range(2147483638, 10)));
            Assert.IsTrue(Range(^2147483600..100).SequenceEqual(source[48..100]));
            Assert.IsTrue(Range(2..7).SequenceEqual(source[2..7]));
            Assert.IsTrue(Range(2..^2147483600).SequenceEqual(source[2..48]));
            Assert.IsTrue(Range(^2147483610..^2147483600).SequenceEqual(source[38..48]));

            // Rang with one index.
            Assert.IsTrue(Range(^48..).SequenceEqual(Enumerable.Range(2147483600, 48)));
            Assert.IsTrue(Range(2147483600..).SequenceEqual(Enumerable.Range(2147483600, 48)));
            Assert.IsTrue(Range(..^2147483600).SequenceEqual(Enumerable.Range(0, 48)));
            Assert.IsTrue(Range(..48).SequenceEqual(Enumerable.Range(0, 48)));

            // All.
            Assert.IsTrue(Range(..).AsEnumerable().Take(100).ToArray().SequenceEqual(source[..]));

            // Single element in the middle.
            Assert.IsTrue(Range(^48..2147483601).SequenceEqual(new int[] { 2147483600 }));
            Assert.IsTrue(Range(48..49).SequenceEqual(new int[] { 48 }));
            Assert.IsTrue(Range(47..^2147483600).SequenceEqual(new int[] { 47 }));
            Assert.IsTrue(Range(^2147483601..^2147483600).SequenceEqual(new int[] { 47 }));

            // Single element at start.
            // Assert.IsTrue((^2147483648..1).SequenceEqual(new int[] { 0 }));
            Assert.IsTrue(Range(0..1).SequenceEqual(new int[] { 0 }));
            Assert.IsTrue(Range(0..^2147483647).SequenceEqual(new int[] { 0 }));
            // Assert.IsTrue((^2147483648..^2147483647).SequenceEqual(new int[] { 0 }));

            // Single element at end.
            // Assert.IsTrue((^1..2147483648).SequenceEqual(new int[] { 2147483647 }));
            // Assert.IsTrue((2147483647..2147483648).SequenceEqual(new int[] { 2147483647 }));
            Assert.IsTrue(Range(2147483647..^0).SequenceEqual(new int[] { 2147483647 }));
            Assert.IsTrue(Range(^1..^0).SequenceEqual(new int[] { 2147483647 }));

            // No element.
            Assert.IsFalse(Range(3..3).AsEnumerable().Any());
            Assert.IsFalse(Range(48..^2147483600).AsEnumerable().Any());
            Assert.IsFalse(Range(^2147483600..48).AsEnumerable().Any());
            Assert.IsFalse(Range(^6..^6).AsEnumerable().Any());

            // Invalid range.
            Assert.ThrowsException<OverflowException>(() => Range(3..2));
            Assert.ThrowsException<OverflowException>(() => Range(50..^2147483600));
            Assert.ThrowsException<OverflowException>(() => Range(^2147483600..8));
            Assert.ThrowsException<OverflowException>(() => Range(^6..^7));
        }
    }
}
