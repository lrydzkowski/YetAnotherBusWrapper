namespace YetAnotherBusWrapper.Consuming;

internal interface IConsumerProcessor : IDisposable, IAsyncDisposable
{
    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
