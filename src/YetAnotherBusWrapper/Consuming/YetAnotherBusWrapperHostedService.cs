using Microsoft.Extensions.Hosting;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Provisioning;

namespace YetAnotherBusWrapper.Consuming;

internal sealed class YetAnotherBusWrapperHostedService : IHostedService, IDisposable, IAsyncDisposable
{
    private readonly ClientRetryOptionsValidator _clientRetryOptionsValidator;
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
    private readonly IReadOnlyList<IConsumerProcessor> _processors;
    private readonly QueueProvisioner _queueProvisioner;
    private readonly SubscriptionProvisioner _subscriptionProvisioner;
    private readonly TopicProvisioner _topicProvisioner;
    private bool _isDisposed;
    private bool _isStarted;

    public YetAnotherBusWrapperHostedService(
        QueueProvisioner queueProvisioner,
        TopicProvisioner topicProvisioner,
        SubscriptionProvisioner subscriptionProvisioner,
        IEnumerable<IConsumerProcessor> processors,
        ClientRetryOptionsValidator clientRetryOptionsValidator
    )
    {
        _queueProvisioner = queueProvisioner;
        _topicProvisioner = topicProvisioner;
        _subscriptionProvisioner = subscriptionProvisioner;
        _processors = processors.ToList();
        _clientRetryOptionsValidator = clientRetryOptionsValidator;
    }

    public async ValueTask DisposeAsync()
    {
        await _lifecycleLock.WaitAsync();
        try
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            await DisposeConsumersAsync();
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _lifecycleLock.WaitAsync(cancellationToken);
        try
        {
            if (_isStarted || _isDisposed)
            {
                return;
            }

            _clientRetryOptionsValidator.EnsureConsistentRetryOptions();
            try
            {
                await ProvisionEntitiesAsync(cancellationToken);
                await StartConsumersAsync(cancellationToken);
            }
            catch
            {
                await RollBackStartupAsync();
                throw;
            }

            _isStarted = true;
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _lifecycleLock.WaitAsync(cancellationToken);
        try
        {
            if (!_isStarted || _isDisposed)
            {
                return;
            }

            _isStarted = false;
            await StopConsumersAsync(cancellationToken);
            await DeleteEntitiesAsync(cancellationToken);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    private async Task ProvisionEntitiesAsync(CancellationToken cancellationToken)
    {
        await _queueProvisioner.CreateQueuesAsync(cancellationToken);
        await _topicProvisioner.CreateTopicsAsync(cancellationToken);
        await _subscriptionProvisioner.CreateSubscriptionsAsync(cancellationToken);
    }

    private async Task StartConsumersAsync(CancellationToken cancellationToken)
    {
        foreach (IConsumerProcessor processor in _processors)
        {
            await processor.StartAsync(cancellationToken);
        }
    }

    private async Task RollBackStartupAsync()
    {
        try
        {
            await StopConsumersAsync(CancellationToken.None);
        }
        finally
        {
            await DeleteEntitiesAsync(CancellationToken.None);
        }
    }

    private async Task StopConsumersAsync(CancellationToken cancellationToken)
    {
        foreach (IConsumerProcessor processor in _processors)
        {
            await processor.StopAsync(cancellationToken);
        }
    }

    private async Task DeleteEntitiesAsync(CancellationToken cancellationToken)
    {
        await _subscriptionProvisioner.DeleteSubscriptionsAsync(cancellationToken);
        await _topicProvisioner.DeleteTopicsAsync(cancellationToken);
        await _queueProvisioner.DeleteQueuesAsync(cancellationToken);
    }

    private async Task DisposeConsumersAsync()
    {
        foreach (IConsumerProcessor processor in _processors)
        {
            await processor.DisposeAsync();
        }
    }
}
