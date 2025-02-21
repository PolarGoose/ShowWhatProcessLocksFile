using NUnit.Framework;
using ShowWhatProcessLocksFile.LockFinding.Utils;

namespace Test.LockFinding.Utils;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public class WorkerThreadWIthDeadLockDetectionTest
{
    [Test]
    public void Action_completes_without_deadlock()
    {
        var worker = new WorkerThreadWithDeadLockDetection(TimeSpan.FromSeconds(300), watchdog =>
        {
            for(var ms = 0; ms <= 600; ms += 50)
            {
                watchdog.Arm();
                Thread.Sleep(ms);
                watchdog.Disarm();
            }
        });

        Assert.That(worker.Run(), Is.True);
    }

    [Test]
    public void Action_deadlocks()
    {
        var worker = new WorkerThreadWithDeadLockDetection(TimeSpan.FromMilliseconds(100), watchdog =>
        {
            watchdog.Arm();
            Thread.Sleep(1000);
            watchdog.Disarm();
        });

        Assert.That(worker.Run(), Is.False);
    }

    [Test]
    public void Action_throws_exception()
    {
        var worker = new WorkerThreadWithDeadLockDetection(TimeSpan.FromMilliseconds(100), watchdog =>
        {
            throw new InvalidOperationException("Test exception");
        });

        Assert.That(worker.Run(), Is.True);
    }

    [Test]
    public void Action_does_not_arm_watchdog()
    {
        var worker = new WorkerThreadWithDeadLockDetection(TimeSpan.FromMilliseconds(100), watchdog =>
        {
            Thread.Sleep(10);
        });
        Assert.That(worker.Run(), Is.True);
    }
}
