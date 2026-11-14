using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Clients;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.Provisioning;

internal sealed class SubscriptionProvisioner
{
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly EntityNameResolver _entityNameResolver;
    private readonly EntityOptionsStore _optionsStore;
    private readonly YetAnotherBusWrapperRegistry _registry;
    private readonly ProvisioningRunner _runner;

    public SubscriptionProvisioner(
        YetAnotherBusWrapperRegistry registry,
        EntityOptionsStore optionsStore,
        ServiceBusClientProvider clientProvider,
        EntityNameResolver entityNameResolver,
        ILogger<SubscriptionProvisioner> logger
    )
    {
        _registry = registry;
        _optionsStore = optionsStore;
        _clientProvider = clientProvider;
        _entityNameResolver = entityNameResolver;
        _runner = new ProvisioningRunner(logger);
    }

    public async Task CreateSubscriptionsAsync(CancellationToken cancellationToken)
    {
        foreach (SubscriptionRegistration registration in _registry.Subscriptions)
        {
            TopicOptions topicOptions = _optionsStore.GetTopic(registration.TopicLogicalEntityName);
            SubscriptionOptions options = _optionsStore.GetSubscription(registration.LogicalEntityName);
            if (!topicOptions.Enabled || !options.Enabled || !options.CreateOnStartup)
            {
                continue;
            }

            await _runner.CreateAsync(CreateEntity(registration, topicOptions, options), cancellationToken);
        }
    }

    public async Task DeleteSubscriptionsAsync(CancellationToken cancellationToken)
    {
        foreach (SubscriptionRegistration registration in _registry.Subscriptions)
        {
            TopicOptions topicOptions = _optionsStore.GetTopic(registration.TopicLogicalEntityName);
            SubscriptionOptions options = _optionsStore.GetSubscription(registration.LogicalEntityName);
            if (!topicOptions.Enabled || !options.Enabled || !options.DeleteOnShutdown)
            {
                continue;
            }

            await _runner.DeleteAsync(CreateEntity(registration, topicOptions, options), cancellationToken);
        }
    }

    private ProvisionedSubscription CreateEntity(
        SubscriptionRegistration registration,
        TopicOptions topicOptions,
        SubscriptionOptions options
    )
    {
        return new ProvisionedSubscription(
            registration,
            topicOptions,
            options,
            _clientProvider.GetAdministrationClient(
                topicOptions.ConnectionString!,
                topicOptions.AdministrationConnectionString,
                topicOptions.ClientRetry
            ),
            _entityNameResolver
        );
    }
}
