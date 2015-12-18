namespace Dixin.Tests.Diagnostics
{
    using System.Collections.Generic;
    using System.Linq;

    using Dixin.Common;
    using Dixin.Diagnostics;
    using Dixin.Management;
    using Dixin.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProcessHelperTests
    {
        [TestMethod]
        public void QueryAllTest()
        {
            IEnumerable<Win32_Process> processes = ProcessHelper.QueryAll();
            EnumerableAssert.Single(processes.Where(process => process.Name.EqualsOrdinal("System Idle Process")));
        }

        [TestMethod]
        public void QueryByNameTest()
        {
            IEnumerable<Win32_Process> processes = ProcessHelper.QueryByName("System Idle Process");
            EnumerableAssert.Single(processes);
        }

        [TestMethod]
        public void QueryByIdTest()
        {
            uint? id = ProcessHelper.QueryByName("System Idle Process").Single().ProcessId;
            Assert.IsNotNull(id);
            IEnumerable<Win32_Process> processes = ProcessHelper.QueryById(id.Value);
            EnumerableAssert.Single(processes);
        }
    }
}
