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
            await Linq.LinqToJson("fuiKNFp9vQFvjLNvx4sUwti4Yb5yGutBN4Xh10LXZhhRKjWlV4");
        }
    }
}
