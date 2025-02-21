namespace ShowWhatProcessLocksFile.LockFinding.Utils;

internal sealed class Watchdog : IDisposable
{
    private readonly Action onTriggered;
    private Timer? timer;
    private readonly TimeSpan timeout;
    private bool isTriggered = false;

    public Watchdog(Action onTriggered, TimeSpan timeout)
    {
        this.onTriggered = onTriggered;
        this.timeout = timeout;
    }

    public void Arm()
    {
        // DebugFileWriters.Log("Arming watchdog");
        timer = new Timer(OnTimerElapsed, null, timeout, Timeout.InfiniteTimeSpan);
    }

    public void Disarm()
    {
        if (timer is null)
        {
            // DebugFileWriters.Log("Disarm failed: not armed");
            throw new InvalidOperationException("Watchdog is not armed");
        }

        if(isTriggered)
        {
            // DebugFileWriters.Log("Disarm failed: already triggered");
            throw new InvalidOperationException("Watchdog has already been triggered");
        }

        // DebugFileWriters.Log("Disarm watchdog");

        timer.Dispose();
        timer = null;
    }

    private void OnTimerElapsed(object? state)
    {
        // DebugFileWriters.Log("OnTimerElapsed");

        isTriggered = true;
        onTriggered();
    }

    void IDisposable.Dispose()
    {
        if(timer is not null)
        {
            // DebugFileWriters.Log("Dispose");
        }

        timer?.Dispose();
        timer = null;
    }
}
