namespace YetAnotherBusWrapper.Consuming;

internal interface IProcessorAdapter : IAsyncDisposable
{
    Task StartProcessingAsync(CancellationToken cancellationToken);

    Task StopProcessingAsync(CancellationToken cancellationToken);
}
