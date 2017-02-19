namespace Tutorial.Tests.Functional
{
    using System.Collections.Generic;
    using Tutorial.Functional;
    using Tutorial.Tests;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Mono.Cecil;

    [TestClass]
    public class VariancesTests
    {
        [TestMethod]
        public void GetTypesWithVarianceTest()
        {
            IEnumerable<TypeDefinition> typesWithVariance = Variances.GetTypesWithVariance();
            EnumerableAssert.Multiple(typesWithVariance);
        }
    }
}
