using YetAnotherBusWrapper.Naming;

namespace YetAnotherBusWrapper.Provisioning;

internal interface IProvisionedEntity
{
    EntityKind Kind { get; }

    string LogicalEntityName { get; }

    string ConfiguredName { get; }

    bool FailStartupOnProvisioningError { get; }

    string ResolveEntityName();

    Task<bool> ExistsAsync(CancellationToken cancellationToken);

    Task CreateAsync(CancellationToken cancellationToken);

    Task DeleteAsync(CancellationToken cancellationToken);
}
