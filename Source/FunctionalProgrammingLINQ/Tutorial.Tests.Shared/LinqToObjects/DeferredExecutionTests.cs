namespace Tutorial.Tests.LinqToObjects
{
    using System.Linq;

    using Tutorial.LinqToObjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DeferredExecutionTests
    {
        [TestMethod]
        public void ReverseTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Reverse(enumerable),
                DeferredExecution.CompiledReverseGenerator(enumerable));

            enumerable = new int[] { };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Reverse(enumerable),
                DeferredExecution.CompiledReverseGenerator(enumerable));
        }

        [TestMethod]
        public void QueryTest()
        {
            DeferredExecution.ForEachWhereAndSelect();
            DeferredExecution.ForEachSelectAndReverse();
        }
    }
}