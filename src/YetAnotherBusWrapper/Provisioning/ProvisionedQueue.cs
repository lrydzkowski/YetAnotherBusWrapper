using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.Provisioning;

internal sealed class ProvisionedQueue : IProvisionedEntity
{
    private readonly ServiceBusAdministrationClient _client;
    private readonly EntityNameResolver _entityNameResolver;
    private readonly QueueOptions _options;
    private readonly QueueRegistration _registration;

    public ProvisionedQueue(
        QueueRegistration registration,
        QueueOptions options,
        ServiceBusAdministrationClient client,
        EntityNameResolver entityNameResolver
    )
    {
        _registration = registration;
        _options = options;
        _client = client;
        _entityNameResolver = entityNameResolver;
    }

    public EntityKind Kind => EntityKind.Queue;

    public string LogicalEntityName => _registration.LogicalEntityName;

    public string ConfiguredName => _options.QueueName!;

    public bool FailStartupOnProvisioningError => _options.FailStartupOnProvisioningError;

    public string ResolveEntityName()
    {
        return _entityNameResolver.ResolveQueueName(_options);
    }

    public async Task<bool> ExistsAsync(CancellationToken cancellationToken)
    {
        return await _client.QueueExistsAsync(ResolveEntityName(), cancellationToken);
    }

    public async Task CreateAsync(CancellationToken cancellationToken)
    {
        await _client.CreateQueueAsync(BuildCreateOptions(), cancellationToken);
    }

    public async Task DeleteAsync(CancellationToken cancellationToken)
    {
        await _client.DeleteQueueAsync(ResolveEntityName(), cancellationToken);
    }

    private CreateQueueOptions BuildCreateOptions()
    {
        CreateQueueOptions createOptions = new(ResolveEntityName()) { MaxDeliveryCount = _options.MaxDeliveryCount };
        if (_options.DuplicateDetectionWindow > TimeSpan.Zero)
        {
            createOptions.RequiresDuplicateDetection = true;
            createOptions.DuplicateDetectionHistoryTimeWindow = _options.DuplicateDetectionWindow;
        }

        _registration.ConfigureCreateOptions?.Invoke(createOptions);

        return createOptions;
    }
}
