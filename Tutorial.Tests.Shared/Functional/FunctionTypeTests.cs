namespace Tutorial.Tests.Functional
{
    using Tutorial.Functional;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FunctionTypeTests
    {
        [TestMethod]
        public void DelegateTest()
        {
            Functions.Static();
        }
    }
}
