using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ShowWhatProcessLocksFile.LockFinding;

namespace Test.LockFinding;

[TestFixture]
public class LockFinderTest
{
    [TestCase(@"C:\PathThatDoesNotExist")]
    [TestCase(@"C:\Windows\system.ini")] // existing but not locked path
    public void Returns_empty_list_If_path_does_mot_exist_or_not_locked(string path)
    {
        var processes = LockFinder.FindWhatProcessesLockPath(path);
        Assert.IsEmpty(processes);
    }

    [TestCase(
        @"C:\Windows",
        "svchost.exe",
        new[] {
            @"C:\Windows\System32\en-US\svchost.exe.mui",
            @"C:\Windows\System32" })]
    [TestCase(
        @"C:\Windows\",
        "svchost.exe",
        new[] {
            @"C:\Windows\System32\en-US\svchost.exe.mui",
            @"C:\Windows\System32" })]
    [TestCase(
        @"C:\Windows",
        "explorer.exe",
        new[] {
            @"C:\Windows\en-US\explorer.exe.mui",
            @"C:\Windows\System32" })]
    public void If_path_is_locked_Returns_information_about_processes_that_lock_this_path(string path, string processName, IEnumerable<string> pathThatShouldBeLocked)
    {
        var processes = LockFinder.FindWhatProcessesLockPath(path).ToList();

        var info = AssertContainsProcessInfo(
            processes,
            p => p.ProcessName == processName,
            $"{processName} process should lock files in the '{path}'");
        foreach (var p in pathThatShouldBeLocked)
        {
            AssertLocksPath(info, p);
        }
    }

    [Test]
    public void Returns_only_information_related_to_the_requested_path()
    {
        var processes = LockFinder.FindWhatProcessesLockPath(@"C:\Program Files").ToList();
        foreach (var lockedPath in processes.SelectMany(proc => proc.LockedPath))
        {
            StringAssert.DoesNotStartWith(@"C:\Program Files (x86)", lockedPath);
        }
    }

    private static ProcessInfo AssertContainsProcessInfo(IEnumerable<ProcessInfo> processes, Predicate<ProcessInfo> condition, string errorMessage = null)
    {
        foreach (var proc in processes)
        {
            if (condition(proc))
            {
                return proc;
            }
        }
        throw new AssertionException(errorMessage);
    }

    private static void AssertLocksPath(ProcessInfo process, string lockedPath)
    {
        var path = process.LockedPath.FirstOrDefault(p => p == lockedPath);
        Assert.IsNotNull(path,
            $"{process}\ndoesn't lock the path '{lockedPath}'.\nIt only locks the following paths:\n* {string.Join("\n* ", process.LockedPath)}");
    }
}
