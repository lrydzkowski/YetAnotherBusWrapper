namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public sealed class BusyConsumerSignal
{
    private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public BusyConsumerSignal(TimeSpan busyDuration)
    {
        BusyDuration = busyDuration;
    }

    public TimeSpan BusyDuration { get; }

    public void MarkStarted()
    {
        _started.TrySetResult();
    }

    public Task WaitUntilStartedAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        return _started.Task.WaitAsync(timeout, cancellationToken);
    }
}
