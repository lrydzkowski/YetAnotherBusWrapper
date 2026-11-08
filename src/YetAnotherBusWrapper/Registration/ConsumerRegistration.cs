namespace YetAnotherBusWrapper.Registration;

internal sealed record ConsumerRegistration(Type MessageType, Type ConsumerType, string LogicalEntityName);
