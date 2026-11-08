using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;
using YetAnotherBusWrapper.Tests.Integration.Common.Logging;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Context;

public sealed class TestCaseScope : IAsyncDisposable
{
    private readonly ServiceBusEmulatorFixture _emulator;

    public TestCaseScope(ServiceBusEmulatorFixture emulator, string feature, int testCaseId)
    {
        _emulator = emulator;
        EntityNamePrefix = $"{feature}-t{testCaseId:D3}".ToLowerInvariant();
    }

    public string EntityNamePrefix { get; }

    public string ConnectionString => _emulator.ConnectionString;

    public string AdministrationConnectionString => _emulator.AdministrationConnectionString;

    public EmulatorInspector Inspector => _emulator.Inspector;

    public LogCollector Logs { get; } = new();

    public async ValueTask DisposeAsync()
    {
        await DeleteQueuesAsync(CancellationToken.None);
        await DeleteTopicsAsync(CancellationToken.None);
    }

    public string GetQueueName(string name)
    {
        return $"{EntityNamePrefix}-{name}";
    }

    public async Task<string> CreateQueueAsync(string name, CancellationToken cancellationToken)
    {
        string queueName = GetQueueName(name);
        await Inspector.CreateQueueAsync(queueName, cancellationToken);

        return queueName;
    }

    public async Task<List<string>> GetQueueNamesAsync(CancellationToken cancellationToken)
    {
        return await Inspector.GetQueueNamesAsync($"{EntityNamePrefix}-", cancellationToken);
    }

    public async Task DeleteQueuesAsync(CancellationToken cancellationToken)
    {
        foreach (string queueName in await GetQueueNamesAsync(cancellationToken))
        {
            await Inspector.DeleteQueueIfExistsAsync(queueName, cancellationToken);
        }
    }

    public string GetTopicName(string name)
    {
        return $"{EntityNamePrefix}-{name}";
    }

    public async Task<string> CreateTopicAsync(string name, CancellationToken cancellationToken)
    {
        string topicName = GetTopicName(name);
        await Inspector.CreateTopicAsync(topicName, cancellationToken);

        return topicName;
    }

    public async Task<List<string>> GetTopicNamesAsync(CancellationToken cancellationToken)
    {
        return await Inspector.GetTopicNamesAsync($"{EntityNamePrefix}-", cancellationToken);
    }

    public async Task DeleteTopicsAsync(CancellationToken cancellationToken)
    {
        foreach (string topicName in await GetTopicNamesAsync(cancellationToken))
        {
            await Inspector.DeleteTopicIfExistsAsync(topicName, cancellationToken);
        }
    }
}
