namespace Dixin.Tests.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;

    using Dixin.Linq.Fundamentals;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ReflectionHelperTests
    {
        [TestMethod]
        public void GetTypesWithVarianceTest()
        {
            IEnumerable<Type> typesWithVariance = ReflectionHelper.GetTypesWithVariance();
            EnumerableAssert.Multiple(typesWithVariance);
        }
    }
}
