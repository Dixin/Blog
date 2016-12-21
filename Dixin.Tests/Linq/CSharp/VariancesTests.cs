namespace Dixin.Tests.Linq.CSharp
{
    using System.Collections.Generic;
    using Dixin.Linq.CSharp;
    using Dixin.Linq.Tests;

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
