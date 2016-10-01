namespace Dixin.Tests.Linq.LinqToObjects
{
    using System.Linq;

    using Dixin.Linq.LinqToObjects;
    using Dixin.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DeferredExecutionTests
    {
        [TestMethod]
        public void Reverse2Test()
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
    }
}