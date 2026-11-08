using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.Options;

internal sealed class ClientRetryOptionsValidator
{
    private readonly IOptionsMonitor<QueueOptions> _queueOptionsMonitor;
    private readonly YetAnotherBusWrapperRegistry _registry;
    private readonly IOptionsMonitor<TopicOptions> _topicOptionsMonitor;

    public ClientRetryOptionsValidator(
        YetAnotherBusWrapperRegistry registry,
        IOptionsMonitor<QueueOptions> queueOptionsMonitor,
        IOptionsMonitor<TopicOptions> topicOptionsMonitor
    )
    {
        _registry = registry;
        _queueOptionsMonitor = queueOptionsMonitor;
        _topicOptionsMonitor = topicOptionsMonitor;
    }

    public void EnsureConsistentRetryOptions()
    {
        Dictionary<string, ClientUsage> firstUsages = new(StringComparer.Ordinal);
        foreach (ClientUsage usage in GetEnabledClientUsages())
        {
            if (!firstUsages.TryAdd(usage.ConnectionString, usage))
            {
                EnsureSameRetryOptions(firstUsages[usage.ConnectionString], usage);
            }
        }
    }

    private IEnumerable<ClientUsage> GetEnabledClientUsages()
    {
        foreach (QueueRegistration registration in _registry.Queues)
        {
            QueueOptions options = _queueOptionsMonitor.Get(registration.LogicalEntityName);
            if (!options.Enabled)
            {
                continue;
            }

            yield return ClientUsage.From(
                EntityKind.Queue,
                registration.LogicalEntityName,
                options.ConnectionString!,
                options.ClientRetry
            );
        }

        foreach (TopicRegistration registration in _registry.Topics)
        {
            TopicOptions options = _topicOptionsMonitor.Get(registration.LogicalEntityName);
            if (!options.Enabled)
            {
                continue;
            }

            yield return ClientUsage.From(
                EntityKind.Topic,
                registration.LogicalEntityName,
                options.ConnectionString!,
                options.ClientRetry
            );
        }
    }

    private static void EnsureSameRetryOptions(ClientUsage first, ClientUsage other)
    {
        if (first.RetrySettings == other.RetrySettings)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Client retry options differ between {first.Entity} and {other.Entity}, which use the same connection "
            + "string. Registrations that share a connection string share one client, so their client retry options "
            + "must match."
        );
    }

    private sealed record ClientUsage(string Entity, string ConnectionString, RetrySettings RetrySettings)
    {
        public static ClientUsage From(
            EntityKind kind,
            string logicalEntityName,
            string connectionString,
            ClientRetryOptions retryOptions
        )
        {
            return new ClientUsage(
                $"{kind.ToLogName()} '{logicalEntityName}'",
                connectionString,
                new RetrySettings(retryOptions.MaxRetries, retryOptions.TryTimeout, retryOptions.MaxDelay)
            );
        }
    }

    private sealed record RetrySettings(int MaxRetries, TimeSpan TryTimeout, TimeSpan MaxDelay);
}
