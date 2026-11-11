namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public sealed class BusyConsumerSignal
{
    private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public BusyConsumerSignal(TimeSpan busyDuration, bool observesCancellation)
    {
        BusyDuration = busyDuration;
        ObservesCancellation = observesCancellation;
    }

    public TimeSpan BusyDuration { get; }

    public bool ObservesCancellation { get; }

    public void MarkStarted()
    {
        _started.TrySetResult();
    }

    public Task WaitUntilStartedAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        return _started.Task.WaitAsync(timeout, cancellationToken);
    }
}
