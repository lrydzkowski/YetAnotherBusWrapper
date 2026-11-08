using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.Provisioning;

internal sealed class ProvisionedSubscription : IProvisionedEntity
{
    private readonly ServiceBusAdministrationClient _client;
    private readonly EntityNameResolver _entityNameResolver;
    private readonly SubscriptionOptions _options;
    private readonly SubscriptionRegistration _registration;
    private readonly TopicOptions _topicOptions;

    public ProvisionedSubscription(
        SubscriptionRegistration registration,
        TopicOptions topicOptions,
        SubscriptionOptions options,
        ServiceBusAdministrationClient client,
        EntityNameResolver entityNameResolver
    )
    {
        _registration = registration;
        _topicOptions = topicOptions;
        _options = options;
        _client = client;
        _entityNameResolver = entityNameResolver;
    }

    public EntityKind Kind => EntityKind.Subscription;

    public string LogicalEntityName => _registration.LogicalEntityName;

    public string ConfiguredName => $"{_topicOptions.TopicName}/{_options.SubscriptionName}";

    public bool FailStartupOnProvisioningError => _options.FailStartupOnProvisioningError;

    public string ResolveEntityName()
    {
        return $"{ResolveTopicName()}/{ResolveSubscriptionName()}";
    }

    public async Task<bool> ExistsAsync(CancellationToken cancellationToken)
    {
        return await _client.SubscriptionExistsAsync(ResolveTopicName(), ResolveSubscriptionName(), cancellationToken);
    }

    public async Task CreateAsync(CancellationToken cancellationToken)
    {
        await _client.CreateSubscriptionAsync(BuildCreateOptions(), cancellationToken);
    }

    public async Task DeleteAsync(CancellationToken cancellationToken)
    {
        await _client.DeleteSubscriptionAsync(ResolveTopicName(), ResolveSubscriptionName(), cancellationToken);
    }

    private string ResolveTopicName()
    {
        return _entityNameResolver.ResolveTopicName(_topicOptions);
    }

    private string ResolveSubscriptionName()
    {
        return _entityNameResolver.ResolveSubscriptionName(_options);
    }

    private CreateSubscriptionOptions BuildCreateOptions()
    {
        CreateSubscriptionOptions createOptions = new(ResolveTopicName(), ResolveSubscriptionName())
        {
            MaxDeliveryCount = _options.MaxDeliveryCount
        };
        _registration.ConfigureCreateOptions?.Invoke(createOptions);

        return createOptions;
    }
}
