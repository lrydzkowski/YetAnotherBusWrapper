using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using YetAnotherBusWrapper.Clients;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.HealthChecks;

internal sealed class EntityHealthChecker
{
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly EntityNameResolver _entityNameResolver;
    private readonly EntityOptionsStore _optionsStore;

    public EntityHealthChecker(
        EntityOptionsStore optionsStore,
        ServiceBusClientProvider clientProvider,
        EntityNameResolver entityNameResolver
    )
    {
        _optionsStore = optionsStore;
        _clientProvider = clientProvider;
        _entityNameResolver = entityNameResolver;
    }

    public Task<HealthCheckResult> CheckQueueAsync(string logicalEntityName, CancellationToken cancellationToken)
    {
        return RunCheckAsync(
            EntityKind.Queue,
            logicalEntityName,
            () => GetQueueStateAsync(logicalEntityName, cancellationToken)
        );
    }

    public Task<HealthCheckResult> CheckTopicAsync(string logicalEntityName, CancellationToken cancellationToken)
    {
        return RunCheckAsync(
            EntityKind.Topic,
            logicalEntityName,
            () => GetTopicStateAsync(logicalEntityName, cancellationToken)
        );
    }

    public Task<HealthCheckResult> CheckSubscriptionAsync(
        SubscriptionRegistration registration,
        CancellationToken cancellationToken
    )
    {
        return RunCheckAsync(
            EntityKind.Subscription,
            registration.LogicalEntityName,
            () => GetSubscriptionStateAsync(registration, cancellationToken)
        );
    }

    private static async Task<HealthCheckResult> RunCheckAsync(
        EntityKind kind,
        string logicalEntityName,
        Func<Task<EntityState>> getStateAsync
    )
    {
        string entity = $"{kind.ToLogName()} '{logicalEntityName}'";
        try
        {
            EntityState state = await getStateAsync();
            if (state.IsDisabled)
            {
                return HealthCheckResult.Degraded($"The {entity} is disabled.");
            }

            if (!state.EntityExists)
            {
                return HealthCheckResult.Unhealthy($"The {entity} (entity name '{state.EntityName}') doesn't exist.");
            }

            return HealthCheckResult.Healthy($"The {entity} (entity name '{state.EntityName}') exists.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy(exception.Message, exception);
        }
    }

    private async Task<EntityState> GetQueueStateAsync(string logicalEntityName, CancellationToken cancellationToken)
    {
        QueueOptions options = _optionsStore.GetQueue(logicalEntityName);
        if (!options.Enabled)
        {
            return EntityState.Disabled;
        }

        string queueName = _entityNameResolver.ResolveQueueName(options);
        if (options.HealthCheck == HealthCheckKind.Peek)
        {
            ServiceBusClient client = _clientProvider.GetClient(options.ConnectionString!, options.ClientRetry);
            await PeekAsync(client.CreateReceiver(queueName), cancellationToken);

            return EntityState.Found(queueName, true);
        }

        ServiceBusAdministrationClient administrationClient = _clientProvider.GetAdministrationClient(
            options.ConnectionString!,
            options.AdministrationConnectionString,
            options.ClientRetry
        );

        return EntityState.Found(queueName, await administrationClient.QueueExistsAsync(queueName, cancellationToken));
    }

    private async Task<EntityState> GetTopicStateAsync(string logicalEntityName, CancellationToken cancellationToken)
    {
        TopicOptions options = _optionsStore.GetTopic(logicalEntityName);
        if (!options.Enabled)
        {
            return EntityState.Disabled;
        }

        string topicName = _entityNameResolver.ResolveTopicName(options);
        ServiceBusAdministrationClient administrationClient = _clientProvider.GetAdministrationClient(
            options.ConnectionString!,
            options.AdministrationConnectionString,
            options.ClientRetry
        );

        return EntityState.Found(topicName, await administrationClient.TopicExistsAsync(topicName, cancellationToken));
    }

    private async Task<EntityState> GetSubscriptionStateAsync(
        SubscriptionRegistration registration,
        CancellationToken cancellationToken
    )
    {
        TopicOptions topicOptions = _optionsStore.GetTopic(registration.TopicLogicalEntityName);
        SubscriptionOptions options = _optionsStore.GetSubscription(registration.LogicalEntityName);
        if (!topicOptions.Enabled || !options.Enabled)
        {
            return EntityState.Disabled;
        }

        string topicName = _entityNameResolver.ResolveTopicName(topicOptions);
        string subscriptionName = _entityNameResolver.ResolveSubscriptionName(options);
        string entityName = $"{topicName}/{subscriptionName}";
        if (options.HealthCheck == HealthCheckKind.Peek)
        {
            ServiceBusClient client = _clientProvider.GetClient(
                topicOptions.ConnectionString!,
                topicOptions.ClientRetry
            );
            await PeekAsync(client.CreateReceiver(topicName, subscriptionName), cancellationToken);

            return EntityState.Found(entityName, true);
        }

        ServiceBusAdministrationClient administrationClient = _clientProvider.GetAdministrationClient(
            topicOptions.ConnectionString!,
            topicOptions.AdministrationConnectionString,
            topicOptions.ClientRetry
        );
        bool exists = await administrationClient.SubscriptionExistsAsync(
            topicName,
            subscriptionName,
            cancellationToken
        );

        return EntityState.Found(entityName, exists);
    }

    private static async Task PeekAsync(ServiceBusReceiver receiver, CancellationToken cancellationToken)
    {
        await using (receiver)
        {
            await receiver.PeekMessageAsync(cancellationToken: cancellationToken);
        }
    }

    private sealed record EntityState(bool IsDisabled, string? EntityName, bool EntityExists)
    {
        public static EntityState Disabled { get; } = new(true, null, false);

        public static EntityState Found(string entityName, bool exists)
        {
            return new EntityState(false, entityName, exists);
        }
    }
}
