namespace Dixin.Tests.Linq.LinqToXml
{
    using Dixin.Linq.LinqToXml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ManipulationTests
    {
        [TestMethod]
        public void ManipulationTest()
        {
            Manipulation.AddChildNode();
            Manipulation.Delete();
            Manipulation.Clone();
        }

        [TestMethod]
        public void TransformTest()
        {
            Manipulation.XslTransform();
            Manipulation.Transform();
        }

        [TestMethod]
        public void ValidateTest()
        {
            Manipulation.Validate();
        }
    }
}
