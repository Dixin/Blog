namespace Dixin.Tests.Linq.CSharp
{
    using Dixin.Linq.CSharp;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ParametersTests
    {
        [TestMethod]
        public void CallerInfoTest()
        {
            Functions.CallTraceWithCaller();
        }
    }
}
