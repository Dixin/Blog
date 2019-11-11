namespace Dixin.Tests
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public static class TestAssembly
    {
        [AssemblyInitialize]
        public static void DataDirectory
            (TestContext testContext)
        {
            AppDomain.CurrentDomain.SetData(
                nameof(DataDirectory),
                new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data")).FullName);
        }
    }
}
