namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public sealed class ConcurrencyTracker
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(50);

    private readonly Lock _lock = new();
    private int _concurrentDeliveries;
    private int _maxConcurrentDeliveries;

    public int MaxConcurrentDeliveries
    {
        get
        {
            lock (_lock)
            {
                return _maxConcurrentDeliveries;
            }
        }
    }

    public async Task HoldAsync(HoldPlan holdPlan, CancellationToken cancellationToken)
    {
        Enter();
        try
        {
            await WaitForConcurrencyAsync(holdPlan, cancellationToken);
        }
        finally
        {
            Exit();
        }
    }

    private void Enter()
    {
        lock (_lock)
        {
            _concurrentDeliveries++;
            _maxConcurrentDeliveries = Math.Max(_maxConcurrentDeliveries, _concurrentDeliveries);
        }
    }

    private void Exit()
    {
        lock (_lock)
        {
            _concurrentDeliveries--;
        }
    }

    private int ReadConcurrentDeliveries()
    {
        lock (_lock)
        {
            return _concurrentDeliveries;
        }
    }

    private async Task WaitForConcurrencyAsync(HoldPlan holdPlan, CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = new(holdPlan.Timeout);
        while (ReadConcurrentDeliveries() < holdPlan.TargetConcurrency
               && !timeoutSource.IsCancellationRequested)
        {
            await Task.Delay(PollInterval, cancellationToken);
        }
    }
}
