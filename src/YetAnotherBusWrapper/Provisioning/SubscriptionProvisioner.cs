using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Clients;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.Provisioning;

internal sealed class SubscriptionProvisioner
{
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly EntityNameResolver _entityNameResolver;
    private readonly YetAnotherBusWrapperRegistry _registry;
    private readonly ProvisioningRunner _runner;
    private readonly IOptionsMonitor<SubscriptionOptions> _subscriptionOptionsMonitor;
    private readonly IOptionsMonitor<TopicOptions> _topicOptionsMonitor;

    public SubscriptionProvisioner(
        YetAnotherBusWrapperRegistry registry,
        IOptionsMonitor<TopicOptions> topicOptionsMonitor,
        IOptionsMonitor<SubscriptionOptions> subscriptionOptionsMonitor,
        ServiceBusClientProvider clientProvider,
        EntityNameResolver entityNameResolver,
        ILogger<SubscriptionProvisioner> logger
    )
    {
        _registry = registry;
        _topicOptionsMonitor = topicOptionsMonitor;
        _subscriptionOptionsMonitor = subscriptionOptionsMonitor;
        _clientProvider = clientProvider;
        _entityNameResolver = entityNameResolver;
        _runner = new ProvisioningRunner(logger);
    }

    public async Task CreateSubscriptionsAsync(CancellationToken cancellationToken)
    {
        foreach (SubscriptionRegistration registration in _registry.Subscriptions)
        {
            TopicOptions topicOptions = _topicOptionsMonitor.Get(registration.TopicLogicalEntityName);
            SubscriptionOptions options = _subscriptionOptionsMonitor.Get(registration.LogicalEntityName);
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
            TopicOptions topicOptions = _topicOptionsMonitor.Get(registration.TopicLogicalEntityName);
            SubscriptionOptions options = _subscriptionOptionsMonitor.Get(registration.LogicalEntityName);
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
                topicOptions.AdministrationConnectionString
            ),
            _entityNameResolver
        );
    }
}
