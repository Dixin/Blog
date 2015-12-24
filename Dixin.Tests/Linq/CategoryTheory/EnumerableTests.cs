namespace Dixin.Tests.Linq.CategoryTheory
{
    using System.Collections.Generic;
    using System.Linq;

    using Dixin.Linq.CategoryTheory;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public partial class MonadTests
    {
        [TestMethod]
        public void EnumerableMonoidTest()
        {
            // Left unit law: μ(η ∘ F) == F
            EnumerableAssert.AreEqual(
                new Enumerable<int>(1).Enumerable().Flatten(),
                new Enumerable<int>(1));

            // Right unit law: F == μ(F ∘ η)
            EnumerableAssert.AreEqual(
                new Enumerable<int>(1),
                new Enumerable<IEnumerable<int>>(1.Enumerable()).Flatten());

            // Associative law: μ(F ∘ F) ∘ F) == F ∘ μ(F ∘ F)
            IEnumerable<Enumerable<int>> left = new Enumerable<int>(1).Enumerable().Enumerable().Flatten();
            IEnumerable<IEnumerable<int>> right = new Enumerable<IEnumerable<int>>(new Enumerable<int>(1)).Flatten().Enumerable();
            Assert.AreEqual(left.Count(), right.Count());
            for (int i = 0; i < left.Count(); i++)
            {
                EnumerableAssert.AreEqual(left.Skip(i - 1).Take(1).Single(), right.Skip(i - 1).Take(1).Single());
            }
        }
    }
}
