namespace Tutorial.Tests.Introduction
{
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.Introduction;

    [TestClass]
    public class LinqTests
    {
        [TestMethod]
        public async Task LinqToJsonTest()
        {
            await LinqToJson.QueryExpression("fuiKNFp9vQFvjLNvx4sUwti4Yb5yGutBN4Xh10LXZhhRKjWlV4");
            await LinqToJson.QueryMethods("fuiKNFp9vQFvjLNvx4sUwti4Yb5yGutBN4Xh10LXZhhRKjWlV4");
        }
    }
}
