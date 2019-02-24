namespace Linq.Range.Tests
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ElementsInTests
    {
        [TestMethod]
        public void NonEmptySourceTests()
        {
            int[] source = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // Multiple elements in the middle.
            Assert.IsTrue(source.ElementsIn(^9..5).ToArray().SequenceEqual(source[^9..5]));
            Assert.IsTrue(source.ElementsIn(2..7).ToArray().SequenceEqual(source[2..7]));
            Assert.IsTrue(source.ElementsIn(2..^4).ToArray().SequenceEqual(source[2..^4]));
            Assert.IsTrue(source.ElementsIn(^7..^4).ToArray().SequenceEqual(source[^7..^4]));

            Assert.IsTrue(source.Hide().ElementsIn(^9..5).ToArray().SequenceEqual(source[^9..5]));
            Assert.IsTrue(source.Hide().ElementsIn(2..7).ToArray().SequenceEqual(source[2..7]));
            Assert.IsTrue(source.Hide().ElementsIn(2..^4).ToArray().SequenceEqual(source[2..^4]));
            Assert.IsTrue(source.Hide().ElementsIn(^7..^4).ToArray().SequenceEqual(source[^7..^4]));

            // Rang with one index.
            Assert.IsTrue(source.ElementsIn(^9..).ToArray().SequenceEqual(source[^9..]));
            Assert.IsTrue(source.ElementsIn(2..).ToArray().SequenceEqual(source[2..]));
            Assert.IsTrue(source.ElementsIn(..^4).ToArray().SequenceEqual(source[..^4]));
            Assert.IsTrue(source.ElementsIn(..6).ToArray().SequenceEqual(source[..6]));

            Assert.IsTrue(source.Hide().ElementsIn(^9..).ToArray().SequenceEqual(source[^9..]));
            Assert.IsTrue(source.Hide().ElementsIn(2..).ToArray().SequenceEqual(source[2..]));
            Assert.IsTrue(source.Hide().ElementsIn(..^4).ToArray().SequenceEqual(source[..^4]));
            Assert.IsTrue(source.Hide().ElementsIn(..6).ToArray().SequenceEqual(source[..6]));

            // All.
            Assert.IsTrue(source.ElementsIn(..).ToArray().SequenceEqual(source[..]));

            Assert.IsTrue(source.Hide().ElementsIn(..).ToArray().SequenceEqual(source[..]));

            // Single element in the middle.
            Assert.IsTrue(source.ElementsIn(^9..2).ToArray().SequenceEqual(source[^9..2]));
            Assert.IsTrue(source.ElementsIn(2..3).ToArray().SequenceEqual(source[2..3]));
            Assert.IsTrue(source.ElementsIn(2..^7).ToArray().SequenceEqual(source[2..^7]));
            Assert.IsTrue(source.ElementsIn(^5..^4).ToArray().SequenceEqual(source[^5..^4]));

            Assert.IsTrue(source.Hide().ElementsIn(^9..2).ToArray().SequenceEqual(source[^9..2]));
            Assert.IsTrue(source.Hide().ElementsIn(2..3).ToArray().SequenceEqual(source[2..3]));
            Assert.IsTrue(source.Hide().ElementsIn(2..^7).ToArray().SequenceEqual(source[2..^7]));
            Assert.IsTrue(source.Hide().ElementsIn(^5..^4).ToArray().SequenceEqual(source[^5..^4]));

            // Single element at start.
            Assert.IsTrue(source.ElementsIn(^10..1).ToArray().SequenceEqual(source[^10..1]));
            Assert.IsTrue(source.ElementsIn(0..1).ToArray().SequenceEqual(source[0..1]));
            Assert.IsTrue(source.ElementsIn(0..^9).ToArray().SequenceEqual(source[0..^9]));
            Assert.IsTrue(source.ElementsIn(^10..^9).ToArray().SequenceEqual(source[^10..^9]));

            Assert.IsTrue(source.Hide().ElementsIn(^10..1).ToArray().SequenceEqual(source[^10..1]));
            Assert.IsTrue(source.Hide().ElementsIn(0..1).ToArray().SequenceEqual(source[0..1]));
            Assert.IsTrue(source.Hide().ElementsIn(0..^9).ToArray().SequenceEqual(source[0..^9]));
            Assert.IsTrue(source.Hide().ElementsIn(^10..^9).ToArray().SequenceEqual(source[^10..^9]));

            // Single element at end.
            Assert.IsTrue(source.ElementsIn(^1..10).ToArray().SequenceEqual(source[^1..10]));
            Assert.IsTrue(source.ElementsIn(9..10).ToArray().SequenceEqual(source[9..10]));
            Assert.IsTrue(source.ElementsIn(9..^0).ToArray().SequenceEqual(source[9..^0]));
            Assert.IsTrue(source.ElementsIn(^1..^0).ToArray().SequenceEqual(source[^1..^0]));

            Assert.IsTrue(source.Hide().ElementsIn(^1..10).ToArray().SequenceEqual(source[^1..10]));
            Assert.IsTrue(source.Hide().ElementsIn(9..10).ToArray().SequenceEqual(source[9..10]));
            Assert.IsTrue(source.Hide().ElementsIn(9..^0).ToArray().SequenceEqual(source[9..^0]));
            Assert.IsTrue(source.Hide().ElementsIn(^1..^0).ToArray().SequenceEqual(source[^1..^0]));

            // No element.
            Assert.IsTrue(source.ElementsIn(3..3).ToArray().SequenceEqual(source[3..3]));
            Assert.IsTrue(source.ElementsIn(6..^4).ToArray().SequenceEqual(source[6..^4]));
            Assert.IsTrue(source.ElementsIn(3..^7).ToArray().SequenceEqual(source[3..^7]));
            Assert.IsTrue(source.ElementsIn(^3..7).ToArray().SequenceEqual(source[^3..7]));
            Assert.IsTrue(source.ElementsIn(^6..^6).ToArray().SequenceEqual(source[^6..^6]));

            Assert.IsTrue(source.Hide().ElementsIn(3..3).ToArray().SequenceEqual(source[3..3]));
            Assert.IsTrue(source.Hide().ElementsIn(6..^4).ToArray().SequenceEqual(source[6..^4]));
            Assert.IsTrue(source.Hide().ElementsIn(3..^7).ToArray().SequenceEqual(source[3..^7]));
            Assert.IsTrue(source.Hide().ElementsIn(^3..7).ToArray().SequenceEqual(source[^3..7]));
            Assert.IsTrue(source.Hide().ElementsIn(^6..^6).ToArray().SequenceEqual(source[^6..^6]));

            // Invalid range.
            Assert.ThrowsException<OverflowException>(() => source[3..2].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(3..2).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[6..^5].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(6..^5).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[3..^8].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(3..^8).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[^6..^7].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(^6..^7).ToArray());

            Assert.ThrowsException<OverflowException>(() => source[3..2].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(3..2).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[6..^5].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(6..^5).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[3..^8].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(3..^8).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[^6..^7].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(^6..^7).ToArray());
        }

        [TestMethod]
        public void EmptySourceTests()
        {
            int[] source = { };

            // Multiple elements in the middle.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..5].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ElementsIn(^9..5).ToArray());
            Assert.ThrowsException<ArgumentException>(() => source[2..7].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.ElementsIn(2..7).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[2..^4].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(2..^4).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^7..^4].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ElementsIn(^7..^4).ToArray());


            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..5].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Hide().ElementsIn(^9..5).ToArray());
            Assert.ThrowsException<ArgumentException>(() => source[2..7].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.Hide().ElementsIn(2..7).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[2..^4].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(2..^4).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^7..^4].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Hide().ElementsIn(^7..^4).ToArray());

            // Rang with one index.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ElementsIn(^9..).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[2..].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(2..).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[..^4].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(..^4).ToArray());
            Assert.ThrowsException<ArgumentException>(() => source[..6].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.ElementsIn(..6).ToArray());

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Hide().ElementsIn(^9..).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[2..].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(2..).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[..^4].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(..^4).ToArray());
            Assert.ThrowsException<ArgumentException>(() => source[..6].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.Hide().ElementsIn(..6).ToArray());

            // All.
            var xx = source[..];
            Assert.IsTrue(source.ElementsIn(..).ToArray().SequenceEqual(source[..]));

            Assert.IsTrue(source.Hide().ElementsIn(..).ToArray().SequenceEqual(source[..]));

            // Single element in the middle.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..2].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ElementsIn(^9..2).ToArray());
            Assert.ThrowsException<ArgumentException>(() => source[2..3].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.ElementsIn(2..3).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[2..^7].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(2..^7).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^5..^4].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ElementsIn(^5..^4).ToArray());

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^9..2].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Hide().ElementsIn(^9..2).ToArray());
            Assert.ThrowsException<ArgumentException>(() => source[2..3].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.Hide().ElementsIn(2..3).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[2..^7].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(2..^7).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^5..^4].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Hide().ElementsIn(^5..^4).ToArray());

            // Single element at start.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^10..1].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ElementsIn(^10..1).ToArray());
            Assert.ThrowsException<ArgumentException>(() => source[0..1].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.ElementsIn(0..1).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[0..^9].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(0..^9).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^10..^9].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ElementsIn(^10..^9).ToArray());

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^10..1].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Hide().ElementsIn(^10..1).ToArray());
            Assert.ThrowsException<ArgumentException>(() => source[0..1].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.Hide().ElementsIn(0..1).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[0..^9].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(0..^9).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^10..^9].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Hide().ElementsIn(^10..^9).ToArray());

            // Single element at end.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^1..10].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ElementsIn(^1..10).ToArray());
            Assert.ThrowsException<ArgumentException>(() => source[9..10].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.ElementsIn(9..10).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[9..^0].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(9..^0).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^1..^0].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ElementsIn(^1..^0).ToArray());

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^1..10].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Hide().ElementsIn(^1..10).ToArray());
            Assert.ThrowsException<ArgumentException>(() => source[9..10].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.Hide().ElementsIn(9..10).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[9..^0].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(9..^0).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^1..^0].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Hide().ElementsIn(^1..^0).ToArray());

            // No element.
            Assert.ThrowsException<ArgumentException>(() => source[3..3].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.ElementsIn(3..3).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[6..^4].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(6..^4).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[3..^7].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(3..^7).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^3..7].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ElementsIn(^3..7).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^6..^6].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ElementsIn(^6..^6).ToArray());

            Assert.ThrowsException<ArgumentException>(() => source[3..3].ToArray());
            Assert.ThrowsException<ArgumentException>(() => source.Hide().ElementsIn(3..3).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[6..^4].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(6..^4).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[3..^7].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(3..^7).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^3..7].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Hide().ElementsIn(^3..7).ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source[^6..^6].ToArray());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Hide().ElementsIn(^6..^6).ToArray());

            // Invalid range.
            Assert.ThrowsException<OverflowException>(() => source[3..2].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(3..2).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[6..^5].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(6..^5).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[3..^8].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(3..^8).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[^6..^7].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.ElementsIn(^6..^7).ToArray());

            Assert.ThrowsException<OverflowException>(() => source[3..2].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(3..2).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[6..^5].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(6..^5).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[3..^8].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(3..^8).ToArray());
            Assert.ThrowsException<OverflowException>(() => source[^6..^7].ToArray());
            Assert.ThrowsException<OverflowException>(() => source.Hide().ElementsIn(^6..^7).ToArray());
        }
    }
}
