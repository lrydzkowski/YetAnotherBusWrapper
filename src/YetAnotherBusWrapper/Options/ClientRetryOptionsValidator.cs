using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.Options;

internal sealed class ClientRetryOptionsValidator
{
    private const string SharedClientRule =
        "which use the same connection string. Registrations that share a connection string share one client";

    private const string SharedAdministrationClientRule =
        "which use the same administration connection string. Registrations that share an administration "
        + "connection string share one administration client";

    private readonly EntityOptionsStore _optionsStore;
    private readonly YetAnotherBusWrapperRegistry _registry;

    public ClientRetryOptionsValidator(
        YetAnotherBusWrapperRegistry registry,
        EntityOptionsStore optionsStore
    )
    {
        _registry = registry;
        _optionsStore = optionsStore;
    }

    public void EnsureConsistentRetryOptions()
    {
        Dictionary<string, ClientUsage> firstUsages = new(StringComparer.Ordinal);
        Dictionary<string, ClientUsage> firstAdministrationUsages = new(StringComparer.Ordinal);
        foreach (ClientUsage usage in GetEnabledClientUsages())
        {
            if (!firstUsages.TryAdd(usage.ConnectionString, usage))
            {
                EnsureSameRetryOptions(firstUsages[usage.ConnectionString], usage, SharedClientRule);
            }

            if (!firstAdministrationUsages.TryAdd(usage.AdministrationConnectionString, usage))
            {
                EnsureSameRetryOptions(
                    firstAdministrationUsages[usage.AdministrationConnectionString],
                    usage,
                    SharedAdministrationClientRule
                );
            }
        }
    }

    private IEnumerable<ClientUsage> GetEnabledClientUsages()
    {
        foreach (QueueRegistration registration in _registry.Queues)
        {
            QueueOptions options = _optionsStore.GetQueue(registration.LogicalEntityName);
            if (!options.Enabled)
            {
                continue;
            }

            yield return ClientUsage.From(
                EntityKind.Queue,
                registration.LogicalEntityName,
                options.ConnectionString!,
                options.AdministrationConnectionString,
                options.ClientRetry
            );
        }

        foreach (TopicRegistration registration in _registry.Topics)
        {
            TopicOptions options = _optionsStore.GetTopic(registration.LogicalEntityName);
            if (!options.Enabled)
            {
                continue;
            }

            yield return ClientUsage.From(
                EntityKind.Topic,
                registration.LogicalEntityName,
                options.ConnectionString!,
                options.AdministrationConnectionString,
                options.ClientRetry
            );
        }
    }

    private static void EnsureSameRetryOptions(ClientUsage first, ClientUsage other, string sharedClientRule)
    {
        if (first.RetrySettings == other.RetrySettings)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Client retry options differ between {first.Entity} and {other.Entity}, {sharedClientRule}, so their "
            + "client retry options must match."
        );
    }

    private sealed record ClientUsage(
        string Entity,
        string ConnectionString,
        string AdministrationConnectionString,
        RetrySettings RetrySettings
    )
    {
        public static ClientUsage From(
            EntityKind kind,
            string logicalEntityName,
            string connectionString,
            string? administrationConnectionString,
            ClientRetryOptions retryOptions
        )
        {
            return new ClientUsage(
                $"{kind.ToLogName()} '{logicalEntityName}'",
                connectionString,
                AdministrationConnectionStringResolver.Resolve(connectionString, administrationConnectionString),
                new RetrySettings(retryOptions.MaxRetries, retryOptions.TryTimeout, retryOptions.MaxDelay)
            );
        }
    }

    private sealed record RetrySettings(int MaxRetries, TimeSpan TryTimeout, TimeSpan MaxDelay);
}
