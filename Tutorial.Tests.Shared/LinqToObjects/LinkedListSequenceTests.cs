namespace Tutorial.Tests.LinqToObjects
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToObjects;

    [TestClass]
    public class LinkedListSequenceTests
    {
        [TestMethod]
        public void EmptyTest()
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
        public void SingleTest()
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
        public void MultipleTest()
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
