namespace Dixin.Tests.Linq.Fundamentals
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using static Dixin.Linq.Fundamentals.Imperative;

    [TestClass]
    public class ImperativeTests
    {
        [TestMethod]
        public void AddTest()
        {
            Assert.AreEqual(3, Add(1, 2));
        }

        [TestMethod]
        public void AddToConsoleTest()
        {
            StringBuilder consoleState = new StringBuilder();
            Console.SetOut(new StringWriter(consoleState));
            DateTime begin = DateTime.Now;
            AddToConsole(1, 2);
            DateTime end = DateTime.Now;
            string consoleMessage = consoleState.ToString(); // 2016-08-13T19:23:17.0278158-07:00 => 3
            Match match = Regex.Match(consoleMessage, @"(\d{4}-\d{2}-\d{2}T\d{2}\:\d{2}\:\d{2}\.\d{7}[+\-]{1}\d{2}\:\d{2}) => (\d+)");
            DateTime consoleDateTime = Convert.ToDateTime(match.Groups[1].Value);
            int consoleSum = Convert.ToInt32(match.Groups[2].Value);
            Assert.IsTrue(begin < consoleDateTime && consoleDateTime < end);
            Assert.AreEqual(3, consoleSum);
        }
    }
}
