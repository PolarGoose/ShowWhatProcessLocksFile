using NUnit.Framework;
using NUnit.Framework.Legacy;
using ShowWhatProcessLocksFile.LockFinding;

namespace Test.LockFinding;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public class LockFinderTest
{
    [TestCase(@"C:\PathThatDoesNotExist")]
    [TestCase(@"C:\Windows\system.ini")] // existing but not locked path
    public void Returns_empty_list_If_path_does_not_exist_or_not_locked(string path)
    {
        var processes = LockFinder.FindWhatProcessesLockPath(path);
        ClassicAssert.IsEmpty(processes);
    }

    [TestCase(
        @"C:\Windows\",
        "svchost.exe",
        new[]
        {
            @"C:\Windows\System32\en-US\svchost.exe.mui"
        })]
    [TestCase(
        @"C:\Windows",
        "explorer.exe",
        new[]
        {
            @"C:\Windows\en-US\explorer.exe.mui",
            @"C:\Windows\en-US\explorer.exe.mUi",
            @"C:\Windows\Fonts\StaticCache.dat",
            @"C:\Windows\System32\",
            @"C:\WINDOWS\SYSTEM32\ntdll.dll"
        })]
    [TestCase(
        @"C:\Windows\System32",
        "explorer.exe",
        new[]
        {
            @"C:\Windows\System32\",
            @"C:\WINDOWS\SYSTEM32\ntdll.dll"
        })]
    [TestCase(
        @"C:\windows",
        "exploRer.exe",
        new[]
        {
            @"C:\Windows\en-US\explorer.exe.mui",
            @"C:\Windows\en-US\explorer.exe.mUi",
            @"C:\Windows\Fonts\StaticCache.dat"
        })]
    [TestCase(
        @"C:/windows",
        "exploRer.exe",
        new[]
        {
            @"C:\Windows\en-US\explorer.exe.mui",
            @"C:\Windows\en-US\explorer.exe.mUi",
            @"C:\Windows\Fonts\StaticCache.dat"
        })]
    [TestCase(
        @"C:\WINDOWS\SYSTEM32\ntdll.dll",
        "explorer.exe",
        new[]
        {
            @"C:\WINDOWS\SYSTEM32\ntdll.dll"
        })]
    [TestCase(
        @"C:\Windows\en-US\explorer.exe.mui",
        "explorer.exe",
        new[]
        {
            @"C:\Windows\en-US\explorer.exe.mui"
        })]
    [TestCase(
        @"C:\",
        "exploRer.exe",
        new[]
        {
            @"C:\Windows\en-US\explorer.exe.mui",
            @"C:\Windows\en-US\explorer.exe.mUi",
            @"C:\Windows\Fonts\StaticCache.dat"
        })]
    [TestCase(
        @"C:/",
        "exploRer.exe",
        new[]
        {
            @"C:\Windows\en-US\explorer.exe.mui",
            @"C:\Windows\en-US\explorer.exe.mUi",
            @"C:\Windows\Fonts\StaticCache.dat"
        })]
    public void If_path_is_locked_Returns_information_about_processes_that_lock_this_path(string path, string processName, IEnumerable<string> pathThatShouldBeLocked)
    {
        var processes = LockFinder.FindWhatProcessesLockPath(path).ToList();

        var info = AssertContainsProcessInfo(
            processes,
            p => string.Equals(p.ProcessName, processName, StringComparison.InvariantCultureIgnoreCase),
            $"{processName} process should lock files in the '{path}'");
        foreach (var p in pathThatShouldBeLocked)
        {
            AssertLocksPath(info, p);
        }

        Assert.That(info.Icon, Is.Not.Null);
        Assert.That(info.ProcessExecutableFullName, Is.Not.Null);
    }

    [Test]
    public void Returns_only_information_related_to_the_requested_path()
    {
        var processes = LockFinder.FindWhatProcessesLockPath(@"C:\Program Files").ToList();
        foreach (var lockedPath in processes.SelectMany(proc => proc.LockedFileFullNames))
        {
            StringAssert.DoesNotStartWith(@"C:\Program Files (x86)", lockedPath);
        }
    }

    private static ProcessInfo AssertContainsProcessInfo(IEnumerable<ProcessInfo> processes, Predicate<ProcessInfo> condition, string? errorMessage = null)
    {
        foreach (var proc in processes)
        {
            if (condition(proc))
            {
                return proc;
            }
        }

        throw new AssertionException(errorMessage!);
    }

    private static void AssertLocksPath(ProcessInfo process, string lockedPath)
    {
        var path = process.LockedFileFullNames.FirstOrDefault(p => string.Equals(p, lockedPath, StringComparison.InvariantCultureIgnoreCase));
        ClassicAssert.IsNotNull(path,
            $"{process}\ndoesn't lock the path '{lockedPath}'.\nIt only locks the following paths:\n* {string.Join("\n* ", process.LockedFileFullNames)}");
    }
}
