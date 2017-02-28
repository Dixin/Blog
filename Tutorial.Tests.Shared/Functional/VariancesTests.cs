namespace Tutorial.Tests.Functional
{
    using System.Collections.Generic;
    using Tutorial.Functional;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Mono.Cecil;

    using EnumerableAssert = Tutorial.LinqToObjects.EnumerableAssert;

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
