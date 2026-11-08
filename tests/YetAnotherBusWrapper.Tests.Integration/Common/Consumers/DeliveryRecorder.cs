using System.Collections.Concurrent;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public sealed class DeliveryRecorder<TMessage>
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    private readonly ConcurrentQueue<RecordedDelivery<TMessage>> _deliveries = new();
    private int _consumerInstanceCount;

    public int RegisterConsumerInstance()
    {
        return Interlocked.Increment(ref _consumerInstanceCount);
    }

    public void Record(RecordedDelivery<TMessage> delivery)
    {
        _deliveries.Enqueue(delivery);
    }

    public List<RecordedDelivery<TMessage>> GetDeliveries()
    {
        return _deliveries.ToList();
    }

    public async Task WaitForDeliveriesAsync(int expectedCount, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = new(timeout);
        while (_deliveries.Count < expectedCount && !timeoutSource.IsCancellationRequested)
        {
            await Task.Delay(PollInterval, cancellationToken);
        }
    }
}
