namespace Tutorial.Tests.Shared.Linq.CSharp
{
    using System.Threading.Tasks;

    using Dixin.Linq.CSharp;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AsyncTests
    {
        [TestMethod]
        public async Task AsyncAwaitTest()
        {
            object value = new object();
            object result = await Functions.CompiledAsync(value);
            Assert.AreEqual(value, result);
        }
    }
}
