using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Clients;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.Provisioning;

internal sealed class TopicProvisioner
{
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly EntityNameResolver _entityNameResolver;
    private readonly EntityOptionsStore _optionsStore;
    private readonly YetAnotherBusWrapperRegistry _registry;
    private readonly ProvisioningRunner _runner;

    public TopicProvisioner(
        YetAnotherBusWrapperRegistry registry,
        EntityOptionsStore optionsStore,
        ServiceBusClientProvider clientProvider,
        EntityNameResolver entityNameResolver,
        ILogger<TopicProvisioner> logger
    )
    {
        _registry = registry;
        _optionsStore = optionsStore;
        _clientProvider = clientProvider;
        _entityNameResolver = entityNameResolver;
        _runner = new ProvisioningRunner(logger);
    }

    public async Task CreateTopicsAsync(CancellationToken cancellationToken)
    {
        foreach (TopicRegistration registration in _registry.Topics)
        {
            TopicOptions options = _optionsStore.GetTopic(registration.LogicalEntityName);
            if (!options.Enabled || !options.CreateOnStartup)
            {
                continue;
            }

            await _runner.CreateAsync(CreateEntity(registration, options), cancellationToken);
        }
    }

    public async Task DeleteTopicsAsync(CancellationToken cancellationToken)
    {
        foreach (TopicRegistration registration in _registry.Topics)
        {
            TopicOptions options = _optionsStore.GetTopic(registration.LogicalEntityName);
            if (!options.Enabled || !options.DeleteOnShutdown)
            {
                continue;
            }

            await _runner.DeleteAsync(CreateEntity(registration, options), cancellationToken);
        }
    }

    private ProvisionedTopic CreateEntity(TopicRegistration registration, TopicOptions options)
    {
        return new ProvisionedTopic(
            registration,
            options,
            _clientProvider.GetAdministrationClient(
                options.ConnectionString!,
                options.AdministrationConnectionString,
                options.ClientRetry
            ),
            _entityNameResolver
        );
    }
}
