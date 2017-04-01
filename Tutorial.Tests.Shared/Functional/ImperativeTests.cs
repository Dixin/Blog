namespace Tutorial.Tests.Functional
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Tutorial.Introduction.Functions;

    [TestClass]
    public class ImperativeTests
    {
        [TestMethod]
        public void AddTest()
        {
            int a = 1;
            int b = 2;
            Assert.AreEqual(a + b, Add(a, b));
        }

        [TestMethod]
        public void AddWithLogTest()
        {
            int a = 1;
            int b = 2;
            StringBuilder consoleOutput = new StringBuilder();
            StringWriter consoleOutputInterceptor = new StringWriter(consoleOutput);
            Console.SetOut(consoleOutputInterceptor);
            Assert.AreEqual(a + b, AddWithLog(a, b)); // 1 + 2 => 3
            Match match = Regex.Match(consoleOutput.ToString(), @"(\d+) \+ (\d+) => (\d+)");
            Assert.AreEqual(a, int.Parse(match.Groups[1].Value));
            Assert.AreEqual(b, int.Parse(match.Groups[2].Value));
            Assert.AreEqual(a + b, int.Parse(match.Groups[3].Value));
        }
    }
}
