namespace Linq.Range.Tests
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SliceTests
    {
        [TestMethod]
        public void NonEmptySourceTests()
        {
            int[] source = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // Multiple elements in the middle.
            Assert.IsTrue(source.Slice(^9..5).ToArray().SequenceEqual(source[^9..5]));
            Assert.IsTrue(source.Slice(2..7).ToArray().SequenceEqual(source[2..7]));
            Assert.IsTrue(source.Slice(2..^4).ToArray().SequenceEqual(source[2..^4]));
            Assert.IsTrue(source.Slice(^7..^4).ToArray().SequenceEqual(source[^7..^4]));

            Assert.IsTrue(source.Hide().Slice(^9..5).ToArray().SequenceEqual(source[^9..5]));
            Assert.IsTrue(source.Hide().Slice(2..7).ToArray().SequenceEqual(source[2..7]));
            Assert.IsTrue(source.Hide().Slice(2..^4).ToArray().SequenceEqual(source[2..^4]));
            Assert.IsTrue(source.Hide().Slice(^7..^4).ToArray().SequenceEqual(source[^7..^4]));

            // Rang with one index.
            Assert.IsTrue(source.Slice(^9..).ToArray().SequenceEqual(source[^9..]));
            Assert.IsTrue(source.Slice(2..).ToArray().SequenceEqual(source[2..]));
            Assert.IsTrue(source.Slice(..^4).ToArray().SequenceEqual(source[..^4]));
            Assert.IsTrue(source.Slice(..6).ToArray().SequenceEqual(source[..6]));

            Assert.IsTrue(source.Hide().Slice(^9..).ToArray().SequenceEqual(source[^9..]));
            Assert.IsTrue(source.Hide().Slice(2..).ToArray().SequenceEqual(source[2..]));
            Assert.IsTrue(source.Hide().Slice(..^4).ToArray().SequenceEqual(source[..^4]));
            Assert.IsTrue(source.Hide().Slice(..6).ToArray().SequenceEqual(source[..6]));

            // All.
            Assert.IsTrue(source.Slice(..).ToArray().SequenceEqual(source[..]));

            Assert.IsTrue(source.Hide().Slice(..).ToArray().SequenceEqual(source[..]));

            // Single element in the middle.
            Assert.IsTrue(source.Slice(^9..2).ToArray().SequenceEqual(source[^9..2]));
            Assert.IsTrue(source.Slice(2..3).ToArray().SequenceEqual(source[2..3]));
            Assert.IsTrue(source.Slice(2..^7).ToArray().SequenceEqual(source[2..^7]));
            Assert.IsTrue(source.Slice(^5..^4).ToArray().SequenceEqual(source[^5..^4]));

            Assert.IsTrue(source.Hide().Slice(^9..2).ToArray().SequenceEqual(source[^9..2]));
            Assert.IsTrue(source.Hide().Slice(2..3).ToArray().SequenceEqual(source[2..3]));
            Assert.IsTrue(source.Hide().Slice(2..^7).ToArray().SequenceEqual(source[2..^7]));
            Assert.IsTrue(source.Hide().Slice(^5..^4).ToArray().SequenceEqual(source[^5..^4]));

            // Single element at start.
            Assert.IsTrue(source.Slice(^10..1).ToArray().SequenceEqual(source[^10..1]));
            Assert.IsTrue(source.Slice(0..1).ToArray().SequenceEqual(source[0..1]));
            Assert.IsTrue(source.Slice(0..^9).ToArray().SequenceEqual(source[0..^9]));
            Assert.IsTrue(source.Slice(^10..^9).ToArray().SequenceEqual(source[^10..^9]));

            Assert.IsTrue(source.Hide().Slice(^10..1).ToArray().SequenceEqual(source[^10..1]));
            Assert.IsTrue(source.Hide().Slice(0..1).ToArray().SequenceEqual(source[0..1]));
            Assert.IsTrue(source.Hide().Slice(0..^9).ToArray().SequenceEqual(source[0..^9]));
            Assert.IsTrue(source.Hide().Slice(^10..^9).ToArray().SequenceEqual(source[^10..^9]));

            // Single element at end.
            Assert.IsTrue(source.Slice(^1..10).ToArray().SequenceEqual(source[^1..10]));
            Assert.IsTrue(source.Slice(9..10).ToArray().SequenceEqual(source[9..10]));
            Assert.IsTrue(source.Slice(9..^0).ToArray().SequenceEqual(source[9..^0]));
            Assert.IsTrue(source.Slice(^1..^0).ToArray().SequenceEqual(source[^1..^0]));

            Assert.IsTrue(source.Hide().Slice(^1..10).ToArray().SequenceEqual(source[^1..10]));
            Assert.IsTrue(source.Hide().Slice(9..10).ToArray().SequenceEqual(source[9..10]));
            Assert.IsTrue(source.Hide().Slice(9..^0).ToArray().SequenceEqual(source[9..^0]));
            Assert.IsTrue(source.Hide().Slice(^1..^0).ToArray().SequenceEqual(source[^1..^0]));

            // No element.
            Assert.IsTrue(source.Slice(3..3).ToArray().SequenceEqual(source[3..3]));
            Assert.IsTrue(source.Slice(6..^4).ToArray().SequenceEqual(source[6..^4]));
            Assert.IsTrue(source.Slice(3..^7).ToArray().SequenceEqual(source[3..^7]));
            Assert.IsTrue(source.Slice(^3..7).ToArray().SequenceEqual(source[^3..7]));
            Assert.IsTrue(source.Slice(^6..^6).ToArray().SequenceEqual(source[^6..^6]));

            Assert.IsTrue(source.Hide().Slice(3..3).ToArray().SequenceEqual(source[3..3]));
            Assert.IsTrue(source.Hide().Slice(6..^4).ToArray().SequenceEqual(source[6..^4]));
            Assert.IsTrue(source.Hide().Slice(3..^7).ToArray().SequenceEqual(source[3..^7]));
            Assert.IsTrue(source.Hide().Slice(^3..7).ToArray().SequenceEqual(source[^3..7]));
            Assert.IsTrue(source.Hide().Slice(^6..^6).ToArray().SequenceEqual(source[^6..^6]));

            // Invalid range.
            Assert.ThrowsException<OverflowException>(() => source[3..2].ToArray());
            Assert.IsFalse(source.Slice(3..2).Any());
            Assert.ThrowsException<OverflowException>(() => source[6..^5].ToArray());
            Assert.IsFalse(source.Slice(6..^5).Any());
            Assert.ThrowsException<OverflowException>(() => source[3..^8].ToArray());
            Assert.IsFalse(source.Slice(3..^8).Any());
            Assert.ThrowsException<OverflowException>(() => source[^6..^7].ToArray());
            Assert.IsFalse(source.Slice(^6..^7).Any());

            Assert.ThrowsException<OverflowException>(() => source[3..2].ToArray());
            Assert.IsFalse(source.Hide().Slice(3..2).Any());
            Assert.ThrowsException<OverflowException>(() => source[6..^5].ToArray());
            Assert.IsFalse(source.Hide().Slice(6..^5).Any());
            Assert.ThrowsException<OverflowException>(() => source[3..^8].ToArray());
            Assert.IsFalse(source.Hide().Slice(3..^8).Any());
            Assert.ThrowsException<OverflowException>(() => source[^6..^7].ToArray());
            Assert.IsFalse(source.Hide().Slice(^6..^7).Any());
        }

        [TestMethod]
        public void EmptySourceTests()
        {
            int[] source = { };

            // Multiple elements in the middle.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..5].ToArray());
            Assert.IsFalse(source.Slice(^9..5).Any());
            Assert.ThrowsException<ArgumentException>(() => source[2..7].ToArray());
            Assert.IsFalse(source.Slice(2..7).Any());
            Assert.ThrowsException<OverflowException>(() => source[2..^4].ToArray());
            Assert.IsFalse(source.Slice(2..^4).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^7..^4].ToArray());
            Assert.IsFalse(source.Slice(^7..^4).Any());

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..5].ToArray());
            Assert.IsFalse(source.Hide().Slice(^9..5).Any());
            Assert.ThrowsException<ArgumentException>(() => source[2..7].ToArray());
            Assert.IsFalse(source.Hide().Slice(2..7).Any());
            Assert.ThrowsException<OverflowException>(() => source[2..^4].ToArray());
            Assert.IsFalse(source.Hide().Slice(2..^4).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^7..^4].ToArray());
            Assert.IsFalse(source.Hide().Slice(^7..^4).Any());

            // Rang with one index.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..].ToArray());
            Assert.IsFalse(source.Slice(^9..).Any());
            Assert.ThrowsException<OverflowException>(() => source[2..].ToArray());
            Assert.IsFalse(source.Slice(2..).Any());
            Assert.ThrowsException<OverflowException>(() => source[..^4].ToArray());
            Assert.IsFalse(source.Slice(..^4).Any());
            Assert.ThrowsException<ArgumentException>(() => source[..6].ToArray());
            Assert.IsFalse(source.Slice(..6).Any());

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..].ToArray());
            Assert.IsFalse(source.Hide().Slice(^9..).Any());
            Assert.ThrowsException<OverflowException>(() => source[2..].ToArray());
            Assert.IsFalse(source.Hide().Slice(2..).Any());
            Assert.ThrowsException<OverflowException>(() => source[..^4].ToArray());
            Assert.IsFalse(source.Hide().Slice(..^4).Any());
            Assert.ThrowsException<ArgumentException>(() => source[..6].ToArray());
            Assert.IsFalse(source.Hide().Slice(..6).Any());

            // All.
            var xx = source[..];
            Assert.IsTrue(source.Slice(..).ToArray().SequenceEqual(source[..]));

            Assert.IsTrue(source.Hide().Slice(..).ToArray().SequenceEqual(source[..]));

            // Single element in the middle.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..2].ToArray());
            Assert.IsFalse(source.Slice(^9..2).Any());
            Assert.ThrowsException<ArgumentException>(() => source[2..3].ToArray());
            Assert.IsFalse(source.Slice(2..3).Any());
            Assert.ThrowsException<OverflowException>(() => source[2..^7].ToArray());
            Assert.IsFalse(source.Slice(2..^7).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^5..^4].ToArray());
            Assert.IsFalse(source.Slice(^5..^4).Any());

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..2].ToArray());
            Assert.IsFalse(source.Hide().Slice(^9..2).Any());
            Assert.ThrowsException<ArgumentException>(() => source[2..3].ToArray());
            Assert.IsFalse(source.Hide().Slice(2..3).Any());
            Assert.ThrowsException<OverflowException>(() => source[2..^7].ToArray());
            Assert.IsFalse(source.Hide().Slice(2..^7).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^5..^4].ToArray());
            Assert.IsFalse(source.Hide().Slice(^5..^4).Any());

            // Single element at start.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^10..1].ToArray());
            Assert.IsFalse(source.Slice(^10..1).Any());
            Assert.ThrowsException<ArgumentException>(() => source[0..1].ToArray());
            Assert.IsFalse(source.Slice(0..1).Any());
            Assert.ThrowsException<OverflowException>(() => source[0..^9].ToArray());
            Assert.IsFalse(source.Slice(0..^9).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^10..^9].ToArray());
            Assert.IsFalse(source.Slice(^10..^9).Any());

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^10..1].ToArray());
            Assert.IsFalse(source.Hide().Slice(^10..1).Any());
            Assert.ThrowsException<ArgumentException>(() => source[0..1].ToArray());
            Assert.IsFalse(source.Hide().Slice(0..1).Any());
            Assert.ThrowsException<OverflowException>(() => source[0..^9].ToArray());
            Assert.IsFalse(source.Hide().Slice(0..^9).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^10..^9].ToArray());
            Assert.IsFalse(source.Hide().Slice(^10..^9).Any());

            // Single element at end.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^1..10].ToArray());
            Assert.IsFalse(source.Slice(^1..^10).Any());
            Assert.ThrowsException<ArgumentException>(() => source[9..10].ToArray());
            Assert.IsFalse(source.Slice(9..10).Any());
            Assert.ThrowsException<OverflowException>(() => source[9..^0].ToArray());
            Assert.IsFalse(source.Slice(9..^9).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^1..^0].ToArray());
            Assert.IsFalse(source.Slice(^1..^9).Any());

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^1..10].ToArray());
            Assert.IsFalse(source.Hide().Slice(^1..^10).Any());
            Assert.ThrowsException<ArgumentException>(() => source[9..10].ToArray());
            Assert.IsFalse(source.Hide().Slice(9..10).Any());
            Assert.ThrowsException<OverflowException>(() => source[9..^0].ToArray());
            Assert.IsFalse(source.Hide().Slice(9..^9).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^1..^0].ToArray());
            Assert.IsFalse(source.Hide().Slice(^1..^9).Any());

            // No element.
            Assert.ThrowsException<ArgumentException>(() => source[3..3].ToArray());
            Assert.IsFalse(source.Slice(3..3).Any());
            Assert.ThrowsException<OverflowException>(() => source[6..^4].ToArray());
            Assert.IsFalse(source.Slice(6..^4).Any());
            Assert.ThrowsException<OverflowException>(() => source[3..^7].ToArray());
            Assert.IsFalse(source.Slice(3..^7).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^3..7].ToArray());
            Assert.IsFalse(source.Slice(^3..7).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^6..^6].ToArray());
            Assert.IsFalse(source.Slice(^6..^6).Any());

            Assert.ThrowsException<ArgumentException>(() => source[3..3].ToArray());
            Assert.IsFalse(source.Hide().Slice(3..3).Any());
            Assert.ThrowsException<OverflowException>(() => source[6..^4].ToArray());
            Assert.IsFalse(source.Hide().Slice(6..^4).Any());
            Assert.ThrowsException<OverflowException>(() => source[3..^7].ToArray());
            Assert.IsFalse(source.Hide().Slice(3..^7).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^3..7].ToArray());
            Assert.IsFalse(source.Hide().Slice(^3..7).Any());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^6..^6].ToArray());
            Assert.IsFalse(source.Hide().Slice(^6..^6).Any());

            // Invalid range.
            Assert.ThrowsException<OverflowException>(() => source[3..2].ToArray());
            Assert.IsFalse(source.Slice(3..2).Any());
            Assert.ThrowsException<OverflowException>(() => source[6..^5].ToArray());
            Assert.IsFalse(source.Slice(6..^5).Any());
            Assert.ThrowsException<OverflowException>(() => source[3..^8].ToArray());
            Assert.IsFalse(source.Slice(3..^8).Any());
            Assert.ThrowsException<OverflowException>(() => source[^6..^7].ToArray());
            Assert.IsFalse(source.Slice(^6..^7).Any());

            Assert.ThrowsException<OverflowException>(() => source[3..2].ToArray());
            Assert.IsFalse(source.Hide().Slice(3..2).Any());
            Assert.ThrowsException<OverflowException>(() => source[6..^5].ToArray());
            Assert.IsFalse(source.Hide().Slice(6..^5).Any());
            Assert.ThrowsException<OverflowException>(() => source[3..^8].ToArray());
            Assert.IsFalse(source.Hide().Slice(3..^8).Any());
            Assert.ThrowsException<OverflowException>(() => source[^6..^7].ToArray());
            Assert.IsFalse(source.Hide().Slice(^6..^7).Any());
        }
    }
}
