using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Clients;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.Provisioning;

internal sealed class QueueProvisioner
{
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly EntityNameResolver _entityNameResolver;
    private readonly EntityOptionsStore _optionsStore;
    private readonly YetAnotherBusWrapperRegistry _registry;
    private readonly ProvisioningRunner _runner;

    public QueueProvisioner(
        YetAnotherBusWrapperRegistry registry,
        EntityOptionsStore optionsStore,
        ServiceBusClientProvider clientProvider,
        EntityNameResolver entityNameResolver,
        ILogger<QueueProvisioner> logger
    )
    {
        _registry = registry;
        _optionsStore = optionsStore;
        _clientProvider = clientProvider;
        _entityNameResolver = entityNameResolver;
        _runner = new ProvisioningRunner(logger);
    }

    public async Task CreateQueuesAsync(CancellationToken cancellationToken)
    {
        foreach (QueueRegistration registration in _registry.Queues)
        {
            QueueOptions options = _optionsStore.GetQueue(registration.LogicalEntityName);
            if (!options.Enabled || !options.CreateOnStartup)
            {
                continue;
            }

            await _runner.CreateAsync(CreateEntity(registration, options), cancellationToken);
        }
    }

    public async Task DeleteQueuesAsync(CancellationToken cancellationToken)
    {
        foreach (QueueRegistration registration in _registry.Queues)
        {
            QueueOptions options = _optionsStore.GetQueue(registration.LogicalEntityName);
            if (!options.Enabled || !options.DeleteOnShutdown)
            {
                continue;
            }

            await _runner.DeleteAsync(CreateEntity(registration, options), cancellationToken);
        }
    }

    private ProvisionedQueue CreateEntity(QueueRegistration registration, QueueOptions options)
    {
        return new ProvisionedQueue(
            registration,
            options,
            _optionsStore.GetQueueConsumer(registration.LogicalEntityName),
            _clientProvider.GetAdministrationClient(options.ConnectionString!, options.AdministrationConnectionString),
            _entityNameResolver
        );
    }
}
