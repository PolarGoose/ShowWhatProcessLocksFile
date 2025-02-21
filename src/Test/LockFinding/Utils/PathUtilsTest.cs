using NUnit.Framework;
using ShowWhatProcessLocksFile.LockFinding.Utils;

namespace Test.LockFinding.Utils;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal class PathUtilsTest
{
    [Test]
    public void AddTrailingSeparatorIfItIsAFolderTest()
    {
        Assert.That(PathUtils.AddTrailingSeparatorIfItIsAFolder(@"C:\Windows\System32"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(PathUtils.AddTrailingSeparatorIfItIsAFolder(@"C:\Windows\System32\"), Is.EqualTo(@"C:\Windows\System32\\"));
        Assert.That(PathUtils.ToCanonicalPath(@"C:\Windows\System32\ntdll.dll"), Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
    }

    [Test]
    public void CanonicalPathTest()
    {
        Assert.That(PathUtils.ToCanonicalPath(@"C:\Windows\System32"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(PathUtils.ToCanonicalPath(@"C:\Windows\System32\"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(PathUtils.ToCanonicalPath(@"C:\Windows\System32\\"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(PathUtils.ToCanonicalPath(@"C:\Windows\\System32/"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(PathUtils.ToCanonicalPath(@"C:/Windows/System32"), Is.EqualTo(@"C:\Windows\System32\"));

        Assert.That(PathUtils.ToCanonicalPath(@"C:/Windows/System32/ntdll.dll"), Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
        Assert.That(PathUtils.ToCanonicalPath(@"C:/Windows/System32//ntdll.dll"), Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
        Assert.That(PathUtils.ToCanonicalPath(@"C:\Windows\\System32\ntdll.dll"), Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
    }
}
