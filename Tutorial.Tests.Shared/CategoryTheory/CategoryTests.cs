namespace Tutorial.Tests.CategoryTheory
{
    using System;

    using Tutorial.CategoryTheory;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using EnumerableAssert = Tutorial.LinqToObjects.EnumerableAssert;

    [TestClass]
    public class CategoryTests
    {
        [TestMethod]
        public void DotNetCategoryObjectsTest()
        {
            DotNetCategory category = new DotNetCategory();
            Type[] types = category.Objects.ToArray();
            EnumerableAssert.Multiple(types);
        }

        [TestMethod]
        public void DotNetCategoryComposeTest()
        {
            DotNetCategory category = new DotNetCategory();
            Func<int, double> function1 = int32 => Math.Sqrt(int32);
            Func<double, string> function2 = @double => @double.ToString("0.00");
            Delegate function = category.Compose(function2, function1);
            Assert.AreEqual("1.41", function.DynamicInvoke(2));
        }
    }
}
