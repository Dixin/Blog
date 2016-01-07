// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DynamicWrapperTest.cs" company="WebOS - http://www.coolwebos.com">
//   Copyright © Dixin 2010 http://weblogs.asp.net/dixin
// </copyright>
// <summary>
//   Defines the DynamicWrapperTest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dixin.Tests.Dynamic
{
    using System.Collections.Generic;
    using System.Linq;

    using Dixin.Dynamic;
    using Dixin.Linq.LinqToSql;

    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DynamicWrapperTest
    {
        [TestMethod]
        public void StaticMemebrTest()
        {
            Assert.AreEqual(0, StaticTest.Value);
            dynamic wrapper = new DynamicWrapper<StaticTest>();

            wrapper.value = 10;
            Assert.AreEqual(10, StaticTest.Value);
            Assert.AreEqual(10, wrapper.Value.ToStatic());

            Assert.AreEqual(2, wrapper.Method().ToStatic());
        }

        [TestMethod]
        public void GetSetIndexFromTypeTest()
        {
            BaseTest @base = new BaseTest();
            Assert.AreEqual("0", @base[5, 5]);
            dynamic wrapper = @base.ToDynamic();
            wrapper[5, 5] = "10";
            Assert.AreEqual("10", @base[5, 5]);
            Assert.AreEqual("10", wrapper[5, 5]);
        }

        [TestMethod]
        public void GetInvokeMemberFromBaseTest()
        {
            Assert.AreEqual(0, new DerivedTest().ToDynamic().array[6, 6]);
            Assert.AreEqual(0, new DerivedTest().ToDynamic().array.ToStatic()[6, 6]);
        }

        [TestMethod]
        public void GetInvokeMemberConvertFromTypeTest()
        {
            using (AdventureWorksDataContext adventureWorks = new AdventureWorksDataContext())
            {
                IQueryable<Product> query =
                    adventureWorks.Products.Where(product => product.ProductID > 0).OrderBy(p => p.Name).Take(2);
                IEnumerable<Product> results =
                    adventureWorks.ToDynamic().Provider.Execute(query.Expression).ReturnValue;
                Assert.IsTrue(results.Any());
            }
        }

        [TestMethod]
        public void ValueTypeTest()
        {
            StructTest test = new StructTest(1);
            dynamic wrapper = test.ToDynamic();
            wrapper.value = 2;
            Assert.AreEqual(2, wrapper.value.ToStatic());
            Assert.AreNotEqual(2, test.Value);

            StructTest test2 = new StructTest(10);
            dynamic wrapper2 = new DynamicWrapper<StructTest>(ref test2);
            wrapper2.value = 20;
            Assert.AreEqual(20, wrapper2.value.ToStatic());
            Assert.AreNotEqual(20, test2.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ValueTypePropertyTest()
        {
            StructTest test2 = new StructTest(10);
            dynamic wrapper2 = new DynamicWrapper<StructTest>(ref test2);

            wrapper2.Value = 30;
        }
    }
}