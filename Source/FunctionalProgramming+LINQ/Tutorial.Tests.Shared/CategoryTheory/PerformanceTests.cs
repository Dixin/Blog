namespace Tutorial.Tests.CategoryTheory
{
    using System.Diagnostics;

    using Tutorial.CategoryTheory;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void SortTest()
        {
            Trace.WriteLine(nameof(Sort.Int32Array));
            Sort.Int32Array();

            Trace.WriteLine(nameof(Sort.StringArray));
            Sort.StringArray();

            Trace.WriteLine(nameof(Sort.ValueTypeArray));
            Sort.ValueTypeArray();

            Trace.WriteLine(nameof(Sort.ReferenceTypeArray));
            Sort.ReferenceTypeArray();
        }

        [TestMethod]
        public void FilterTest()
        {
            Trace.WriteLine(nameof(Filter.Int32Sequence));
            Filter.Int32Sequence();

            Trace.WriteLine(nameof(Filter.StringSequence));
            Filter.StringSequence();

            Trace.WriteLine(nameof(Filter.ValueTypeSequence));
            Filter.ValueTypeSequence();

            Trace.WriteLine(nameof(Filter.ReferenceTypeSequence));
            Filter.ReferenceTypeSequence();
        }

        [TestMethod]
        public void LambdaTest()
        {
            Trace.WriteLine(nameof(Filter.ByPredicate));
            Filter.ByPredicate();
        }
    }
}
