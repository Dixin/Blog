namespace Dixin.Tests.Linq.LinqToObjects
{
    using System.Collections.Generic;
    using Dixin.Linq.LinqToObjects;

    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LinkedListSequenceTests
    {
        [TestMethod]
        public void Empty()
        {
            SinglyLinkedListNode<int> head = null;
            LinkedListSequence<int> sequence = new LinkedListSequence<int>(head);
            List<int> list = new List<int>();
            foreach (int value in sequence)
            {
                list.Add(value);
            }

            Assert.IsFalse(list.Any());
        }

        [TestMethod]
        public void Single()
        {
            SinglyLinkedListNode<int> head = new SinglyLinkedListNode<int>(1);
            LinkedListSequence<int> sequence = new LinkedListSequence<int>(head);
            List<int> list = new List<int>();
            foreach (int value in sequence)
            {
                list.Add(value);
            }

            Assert.AreEqual(1, list.Single());
        }

        [TestMethod]
        public void Multiple()
        {
            SinglyLinkedListNode<int> head = new SinglyLinkedListNode<int>(0, new SinglyLinkedListNode<int>(1, new SinglyLinkedListNode<int>(2, new SinglyLinkedListNode<int>(3))));
            LinkedListSequence<int> sequence = new LinkedListSequence<int>(head);
            List<int> list = new List<int>();
            foreach (int value in sequence)
            {
                list.Add(value);
            }

            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3 }, list);
        }
    }
}
