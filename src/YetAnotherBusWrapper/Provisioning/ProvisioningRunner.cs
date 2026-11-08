using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Naming;

namespace YetAnotherBusWrapper.Provisioning;

internal sealed partial class ProvisioningRunner
{
    private readonly ILogger _logger;

    public ProvisioningRunner(ILogger logger)
    {
        _logger = logger;
    }

    public async Task CreateAsync(IProvisionedEntity entity, CancellationToken cancellationToken)
    {
        string entityName = entity.ConfiguredName;
        try
        {
            entityName = entity.ResolveEntityName();
            await CreateIfMissingAsync(entity, entityName, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogCreationFailed(_logger, exception, entity.Kind.ToLogName(), entity.LogicalEntityName, entityName);
            if (entity.FailStartupOnProvisioningError)
            {
                throw;
            }
        }
    }

    public async Task DeleteAsync(IProvisionedEntity entity, CancellationToken cancellationToken)
    {
        string entityName = entity.ConfiguredName;
        try
        {
            entityName = entity.ResolveEntityName();
            await entity.DeleteAsync(cancellationToken);
            LogDeleted(_logger, entity.Kind.ToLogName(), entity.LogicalEntityName, entityName);
        }
        catch (Exception exception)
        {
            LogDeletionFailed(_logger, exception, entity.Kind.ToLogName(), entity.LogicalEntityName, entityName);
        }
    }

    private async Task CreateIfMissingAsync(
        IProvisionedEntity entity,
        string entityName,
        CancellationToken cancellationToken
    )
    {
        if (await entity.ExistsAsync(cancellationToken))
        {
            LogAlreadyExists(_logger, entity.Kind, entity.LogicalEntityName, entityName);

            return;
        }

        try
        {
            await entity.CreateAsync(cancellationToken);
        }
        catch (ServiceBusException exception)
            when (exception.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
        {
            LogAlreadyExists(_logger, entity.Kind, entity.LogicalEntityName, entityName);

            return;
        }

        LogCreated(_logger, entity.Kind.ToLogName(), entity.LogicalEntityName, entityName);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Created {EntityKind} '{LogicalEntityName}' (entity name '{EntityName}')"
    )]
    private static partial void LogCreated(
        ILogger logger,
        string entityKind,
        string logicalEntityName,
        string entityName
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{EntityKind} '{LogicalEntityName}' already exists (entity name '{EntityName}')"
    )]
    private static partial void LogAlreadyExists(
        ILogger logger,
        EntityKind entityKind,
        string logicalEntityName,
        string entityName
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to create {EntityKind} '{LogicalEntityName}' (entity name '{EntityName}')"
    )]
    private static partial void LogCreationFailed(
        ILogger logger,
        Exception exception,
        string entityKind,
        string logicalEntityName,
        string entityName
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Deleted {EntityKind} '{LogicalEntityName}' (entity name '{EntityName}')"
    )]
    private static partial void LogDeleted(
        ILogger logger,
        string entityKind,
        string logicalEntityName,
        string entityName
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to delete {EntityKind} '{LogicalEntityName}' (entity name '{EntityName}')"
    )]
    private static partial void LogDeletionFailed(
        ILogger logger,
        Exception exception,
        string entityKind,
        string logicalEntityName,
        string entityName
    );
}
