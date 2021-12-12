using NUnit.Framework;
using ShowWhatProcessLocksFile.LockFinding;
using ShowWhatProcessLocksFile.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class TestLockFinder
    {
        [TestCase(@"C:\Program Files")]
        [TestCase(@"C:\Program Files\")]
        public void Retrieve_locking_info_for_ProgramFiles_locked_folder(string path)
        {
            var processes = LockFinder.FindWhatProcessesLockPath(path);
            AssertThatContainsExplorerProcess(processes);
            AssertThatDoesntContainProcessesLockingDifferentPath(processes, path);
        }

        [TestCase(@"C:")]
        [TestCase(@"C:\")]
        public void Retrieve_locking_info_for_disk_C(string path)
        {
            var processes = LockFinder.FindWhatProcessesLockPath(path);
            AssertThatContainsExplorerProcess(processes);
        }

        [TestCase(@"C:\nonExistingFolder")]
        [TestCase(@"C:\nonExistingFolder\")]
        public void Retrieves_nothing_for_folder_which_is_not_locked(string path)
        {
            var processes = LockFinder.FindWhatProcessesLockPath(@"C:\nonExistingFolder\");
            Assert.IsEmpty(processes);
        }

        private void AssertThatContainsExplorerProcess(IEnumerable<ProcessInfo> lockingProcesses)
        {
            var explorerProcesses = lockingProcesses.Where(p => p.Name == "explorer.exe").ToList();
            Assert.GreaterOrEqual(explorerProcesses.Count, 1);
            Assert.IsNotEmpty(explorerProcesses[0].ExecutableFullName);
            Assert.IsNotNull(explorerProcesses[0].Icon);
        }

        private void AssertThatDoesntContainProcessesLockingDifferentPath(IEnumerable<ProcessInfo> lockingProcesses, string requestedPath)
        {
            var otherProcesses = lockingProcesses.Where(p => p.LockedFiles.All(file => !PathUtils.IsInsideFolder(file, requestedPath)));
            Assert.IsEmpty(otherProcesses,
                           "The locking information should only contain information about processes locking a requested path, not some other pathes");
        }
    }
}
