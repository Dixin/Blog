namespace Dixin.Tests
{
#if NETFX
    using System;
    using System.IO;

    using Dixin.Common;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public static class TestAssembly
    {
        [AssemblyInitialize]
        public static void Initialize
            (TestContext testContext) => AppDomainData.DataDirectory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data");
    }
#endif
}
