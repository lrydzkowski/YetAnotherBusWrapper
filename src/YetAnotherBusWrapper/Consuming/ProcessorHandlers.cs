using Azure.Messaging.ServiceBus;

namespace YetAnotherBusWrapper.Consuming;

internal sealed record ProcessorHandlers(
    Func<ServiceBusReceivedMessage, CancellationToken, Task> ProcessMessageAsync,
    Func<ProcessErrorEventArgs, Task> ProcessErrorAsync
);
