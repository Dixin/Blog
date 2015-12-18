// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DynamicWrapperTest.cs" company="WebOS - http://www.coolwebos.com">
//   Copyright © Dixin 2010 http://weblogs.asp.net/dixin
// </copyright>
// <summary>
//   Defines the DynamicWrapperTest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dixin.Dynamic.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Dixin.Dynamic;
    using Dixin.Tests.Dynamic;
    using Dixin.Tests.Properties;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DynamicWrapperTest
    {
        #region Public Methods

        [TestMethod]
        public void Static_Memebr()
        {
            Assert.AreEqual(0, StaticTest.Value);
            dynamic wrapper = new DynamicWrapper<StaticTest>();

            wrapper.value = 10;
            Assert.AreEqual(10, StaticTest.Value);
            Assert.AreEqual(10, wrapper.Value.ToStatic());

            Assert.AreEqual(2, wrapper.Method().ToStatic());
        }

        [TestMethod]
        public void TryGetIndex_TrySetIndex_From_Type()
        {
            BaseTest @base = new BaseTest();
            Assert.AreEqual("0", @base[5, 5]);
            dynamic wrapper = @base.ToDynamic();
            wrapper[5, 5] = "10";
            Assert.AreEqual("10", @base[5, 5]);
            Assert.AreEqual("10", wrapper[5, 5]);
        }

        [TestMethod]
        public void TryGetMember_TryInvokeMember_From_Base()
        {
            Assert.AreEqual(0, new DerivedTest().ToDynamic()._array[6, 6]);
            Assert.AreEqual(0, new DerivedTest().ToDynamic()._array.ToStatic()[6, 6]);
        }

        [TestMethod]
        public void TryGetMember_TryInvokeMember_TryConvert_From_Type()
        {
            using (NorthwindDataContext database = new NorthwindDataContext(Settings.Default.NorthwindConnectionString))
            {
                IQueryable<Product> query =
                    database.Products.Where(product => product.ProductID > 0).OrderBy(p => p.ProductName).Take(2);
                IEnumerable<Product> results =
                    database.ToDynamic().Provider.Execute(query.Expression).ReturnValue;
                Assert.IsTrue(results.Any());
            }
        }

        [TestMethod]
        public void Value_Type()
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
        public void Value_Type_Property()
        {
            StructTest test2 = new StructTest(10);
            dynamic wrapper2 = new DynamicWrapper<StructTest>(ref test2);

            wrapper2.Value = 30;
        }

        #endregion
    }
}