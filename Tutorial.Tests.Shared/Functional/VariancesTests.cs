namespace Tutorial.Tests.Functional
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.Functional;
    using Tutorial.Tests.LinqToObjects;

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
