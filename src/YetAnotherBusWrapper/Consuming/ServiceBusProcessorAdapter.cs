using Azure.Messaging.ServiceBus;

namespace YetAnotherBusWrapper.Consuming;

internal sealed class ServiceBusProcessorAdapter : IProcessorAdapter
{
    private readonly ServiceBusProcessor _processor;

    public ServiceBusProcessorAdapter(ServiceBusProcessor processor, ProcessorHandlers handlers)
    {
        _processor = processor;
        _processor.ProcessMessageAsync += args => handlers.ProcessMessageAsync(args.Message, args.CancellationToken);
        _processor.ProcessErrorAsync += handlers.ProcessErrorAsync;
    }

    public Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        return _processor.StartProcessingAsync(cancellationToken);
    }

    public Task StopProcessingAsync(CancellationToken cancellationToken)
    {
        return _processor.StopProcessingAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _processor.DisposeAsync();
    }
}
