namespace Dixin.Tests.Linq.CSharp
{
    using Dixin.Linq.CSharp;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ParametersTests
    {
        [TestMethod]
        public void ParameterTest()
        {
            Functions.CallPassByValue();
            Functions.CallPassByReference();
            Functions.CallOutput();
            Functions.OutVariable();
        }

        [TestMethod]
        public void CallerInfoTest()
        {
            Functions.CallTraceWithCaller();
        }

        [TestMethod]
        public void ReturnTest()
        {
            Functions.ReturnByValue();
            Functions.ReturnByReference();
        }
    }
}
