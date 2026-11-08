using Azure.Messaging.ServiceBus.Administration;

namespace YetAnotherBusWrapper.Registration;

internal sealed record TopicRegistration(
    Type MessageType,
    string LogicalEntityName,
    Action<CreateTopicOptions>? ConfigureCreateOptions
);
