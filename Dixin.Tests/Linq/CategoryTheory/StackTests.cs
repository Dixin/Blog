namespace Dixin.Tests.Linq.CategoryTheory
{
    using System.Collections.Generic;
    using System.Linq;

    using Dixin.Linq.CategoryTheory;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StackTests
    {
        [TestMethod]
        public void StateMachineTest()
        {
            IEnumerable<int> expected = Enumerable.Range(0, 3).Push(4).Value2.Pop().Value2;
            IEnumerable<int> actual = StateQuery.Stack();
            EnumerableAssert.AreSequentialEqual(expected, actual);
        }
    }
}
