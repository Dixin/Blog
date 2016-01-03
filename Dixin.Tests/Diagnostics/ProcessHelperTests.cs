namespace Dixin.Tests.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics;
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
            IEnumerable<Win32Process> processes = ProcessHelper.All();
            EnumerableAssert.Single(processes.Where(process => process.Name.EqualsOrdinal("System Idle Process")));
        }

        [TestMethod]
        public void QueryByNameTest()
        {
            IEnumerable<Win32Process> processes = ProcessHelper.ByName("System Idle Process");
            EnumerableAssert.Single(processes);
            processes = ProcessHelper.ByName("svchost.exe");
            EnumerableAssert.Multiple(processes);
        }

        [TestMethod]
        public void QueryByIdTest()
        {
            int id = Process.GetProcessesByName("wininit").Single().Id;
            Assert.IsNotNull(id);
            Win32Process process = ProcessHelper.ById((uint)id);
            Assert.IsNotNull(process);
            Assert.AreEqual("wininit.exe", process.Name);
        }

        [TestMethod]
        public void QueryParentProcess()
        {
            uint id = ProcessHelper.ByName("SearchUI.exe").Single().ProcessId.Value; // Cortana.
            Win32Process parentProcess = ProcessHelper.ParentProcess(id);
            Assert.IsNotNull(parentProcess);
        }

        [TestMethod]
        public void QueryAllParentProcesses()
        {
            uint id = ProcessHelper.ByName("SearchUI.exe").Single().ProcessId.Value; // Cortana.
            IEnumerable<Win32Process> parentProcesses = ProcessHelper.AllParentProcess(id);
            EnumerableAssert.Multiple(parentProcesses);
        }

        [TestMethod]
        public void QueryChildProcesses()
        {
            uint id = ProcessHelper.ByName("wininit.exe").Single().ProcessId.Value;
            IEnumerable<Win32Process> childProcesses = ProcessHelper.ChildProcesses(id);
            EnumerableAssert.Multiple(childProcesses);
        }

        [TestMethod]
        public void QueryAllChildProcesses()
        {
            uint id = ProcessHelper.ByName("wininit.exe").Single().ProcessId.Value;
            IEnumerable<Win32Process> tree = ProcessHelper.AllChildProcesses(id);
            EnumerableAssert.Multiple(tree);

            IEnumerable<Win32Process> childProcesses = ProcessHelper.ChildProcesses(id);
            Assert.IsTrue(tree.Count() > childProcesses.Count());
        }
    }
}
