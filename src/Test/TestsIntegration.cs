using NUnit.Framework;
using ShowWhatProcessLocksFile.LockingProcessesInfo;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class TestsIntegration
    {
        [Test]
        public void Retrieve_locking_info_for_ProgramFiles()
        {
            var processes = ProcessesInfoRetriever.GetProcessesInfo(@"C:\Program Files\");
            var explorerProcesses = processes.Where(p => p.Name == "explorer.exe").ToList();
            Assert.GreaterOrEqual(explorerProcesses.Count, 1);
            Assert.IsNotEmpty(explorerProcesses[0].ExecutableFullName);
            Assert.IsNotNull(explorerProcesses[0].Icon);
        }

        [Test]
        public void Retrieve_locking_info_for_C_drive()
        {
            var processes = ProcessesInfoRetriever.GetProcessesInfo(@"C:\");
            var explorerProcesses = processes.Where(p => p.Name == "explorer.exe").ToList();
            Assert.GreaterOrEqual(explorerProcesses.Count, 1);
            Assert.IsNotEmpty(explorerProcesses[0].ExecutableFullName);
            Assert.IsNotNull(explorerProcesses[0].Icon);
        }

        [Test]
        public void Retrieves_nothing_for_folder_which_is_not_locked()
        {
            var processes = ProcessesInfoRetriever.GetProcessesInfo(@"C:\nonExistingFolder");
            Assert.IsEmpty(processes);
        }
    }
}
