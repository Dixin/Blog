namespace Tutorial.Tests.Functional
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.Functional;

    [TestClass]
    public class VariancesTests
    {
        [TestMethod]
        public void GetTypesWithVarianceTest()
        {
            Variances.TypesWithVariance();
        }
    }
}
