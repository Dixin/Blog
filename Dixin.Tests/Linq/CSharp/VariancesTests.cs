namespace Dixin.Tests.Linq.CSharp
{
    using System;
    using System.Collections.Generic;
    using Dixin.Linq.CSharp;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class VariancesTests
    {
        [TestMethod]
        public void GetTypesWithVarianceTest()
        {
            IEnumerable<Type> typesWithVariance = Variances.GetTypesWithVariance();
            EnumerableAssert.Multiple(typesWithVariance);
        }
    }
}
