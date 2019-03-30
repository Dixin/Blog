namespace Tutorial.Tests.LinqToXml
{
    using Tutorial.LinqToXml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ManipulationTests
    {
        [TestMethod]
        public void CloneTest()
        {
            Manipulation.ExplicitClone();
            Manipulation.ImplicitClone();
        }

        [TestMethod]
        public void ManipulationTest()
        {
            Manipulation.Manipulate();
            Manipulation.SetAttributeValue();
            Manipulation.SetElementValue();
            Manipulation.Annotation();
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
            Manipulation.InferSchemas();
            Manipulation.Validate();
            Manipulation.GetSchemaInfo();
        }
    }
}
