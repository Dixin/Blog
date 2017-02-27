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
        public void AddToConsoleTest()
        {
            int a = 1;
            int b = 2;
            StringBuilder consoleState = new StringBuilder();
            Console.SetOut(new StringWriter(consoleState));
            DateTime begin = DateTime.Now;
            AddToConsole(a, b);
            DateTime end = DateTime.Now;
            string consoleMessage = consoleState.ToString(); // 2017-02-13T19:23:17.0278158-08:00 => 3
            Match match = Regex.Match(consoleMessage, @"(\d{4}-\d{2}-\d{2}T\d{2}\:\d{2}\:\d{2}\.\d{7}[+\-]{1}\d{2}\:\d{2}) => (\d+)");
            DateTime consoleDateTime = Convert.ToDateTime(match.Groups[1].Value);
            int consoleSum = Convert.ToInt32(match.Groups[2].Value);
            Assert.IsTrue(begin <= consoleDateTime && consoleDateTime <= end);
            Assert.AreEqual(a + b, consoleSum);
        }
    }
}
