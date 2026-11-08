using Azure.Messaging.ServiceBus.Administration;

namespace YetAnotherBusWrapper.Registration;

internal sealed record QueueRegistration(
    Type MessageType,
    string LogicalEntityName,
    Action<CreateQueueOptions>? ConfigureCreateOptions
);
