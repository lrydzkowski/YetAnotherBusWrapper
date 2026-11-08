using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.Provisioning;

internal sealed class ProvisionedTopic : IProvisionedEntity
{
    private readonly ServiceBusAdministrationClient _client;
    private readonly EntityNameResolver _entityNameResolver;
    private readonly TopicOptions _options;
    private readonly TopicRegistration _registration;

    public ProvisionedTopic(
        TopicRegistration registration,
        TopicOptions options,
        ServiceBusAdministrationClient client,
        EntityNameResolver entityNameResolver
    )
    {
        _registration = registration;
        _options = options;
        _client = client;
        _entityNameResolver = entityNameResolver;
    }

    public EntityKind Kind => EntityKind.Topic;

    public string LogicalEntityName => _registration.LogicalEntityName;

    public string ConfiguredName => _options.TopicName!;

    public bool FailStartupOnProvisioningError => _options.FailStartupOnProvisioningError;

    public string ResolveEntityName()
    {
        return _entityNameResolver.ResolveTopicName(_options);
    }

    public async Task<bool> ExistsAsync(CancellationToken cancellationToken)
    {
        return await _client.TopicExistsAsync(ResolveEntityName(), cancellationToken);
    }

    public async Task CreateAsync(CancellationToken cancellationToken)
    {
        await _client.CreateTopicAsync(BuildCreateOptions(), cancellationToken);
    }

    public async Task DeleteAsync(CancellationToken cancellationToken)
    {
        await _client.DeleteTopicAsync(ResolveEntityName(), cancellationToken);
    }

    private CreateTopicOptions BuildCreateOptions()
    {
        CreateTopicOptions createOptions = new(ResolveEntityName());
        if (_options.DuplicateDetectionWindow > TimeSpan.Zero)
        {
            createOptions.RequiresDuplicateDetection = true;
            createOptions.DuplicateDetectionHistoryTimeWindow = _options.DuplicateDetectionWindow;
        }

        _registration.ConfigureCreateOptions?.Invoke(createOptions);

        return createOptions;
    }
}
