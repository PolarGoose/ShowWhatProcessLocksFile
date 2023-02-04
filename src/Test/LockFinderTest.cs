using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using ShowWhatProcessLocksFile.LockFinding;

namespace Test;

[TestFixture]
public class LockFinderTest
{
    [Test]
    public async Task LockedFolder()
    {
        var res = await LockFinder.FindWhatProcessesLockPath(@"C:\");

        // Check that has a "System" process
        {
            var proc = res.First(p => p.ExecutableFullName == "System");
            Assert.IsNull(proc.Icon);
            Assert.AreEqual(@"NT AUTHORITY\SYSTEM", proc.UserName);
            Assert.AreEqual(4, proc.Pid);
        }

        // Check that has a "svchost" process
        {
            var proc = res.First(p =>
                p.ExecutableFullName == "C:\\Windows\\System32\\svchost.exe"
                && p.UserName == System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            Assert.IsNotNull(proc.Icon);
            Assert.Contains("C:\\Windows\\System32\\en-US\\svchost.exe.mui", proc.LockedFiles.ToList());
        }

        // Check that has a "wininit" process
        {
            var proc = res.First(p => p.ExecutableFullName == "C:\\Windows\\System32\\wininit.exe");
            Assert.IsNotNull(proc.Icon);
            Assert.AreEqual(@"NT AUTHORITY\SYSTEM", proc.UserName);
            Assert.Contains("C:\\Windows\\System32\\en-US\\user32.dll.mui", proc.LockedFiles.ToList());
        }
    }

    [Test]
    public async Task NonLockedFolder()
    {
        var res = await LockFinder.FindWhatProcessesLockPath(@"C:\Users\Public\Documents");
        Assert.IsEmpty(res);
    }

    [Test]
    public void NonExistingFolder()
    {
       Assert.ThrowsAsync<ApplicationException>(async () =>
            await HandleExe.GetProcessesLockingPath(@"C:\NonExistentFolderBlahBlah"));
    }
}
