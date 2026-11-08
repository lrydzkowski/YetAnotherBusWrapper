using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Naming;

namespace YetAnotherBusWrapper.Consuming;

internal sealed partial class ConsumerProcessor : IConsumerProcessor
{
    private readonly Func<ProcessorHandlers, IProcessorAdapter> _createProcessor;
    private readonly IReceivedMessageDispatcher _dispatcher;
    private readonly EntityKind _entityKind;
    private readonly Func<bool> _isEntityEnabled;
    private readonly ILogger<ConsumerProcessor> _logger;
    private readonly string _logicalEntityName;
    private readonly IServiceScopeFactory _scopeFactory;
    private IProcessorAdapter? _processor;

    public ConsumerProcessor(
        EntityKind entityKind,
        string logicalEntityName,
        Func<bool> isEntityEnabled,
        Func<ProcessorHandlers, IProcessorAdapter> createProcessor,
        IReceivedMessageDispatcher dispatcher,
        IServiceScopeFactory scopeFactory,
        ILogger<ConsumerProcessor> logger
    )
    {
        _entityKind = entityKind;
        _logicalEntityName = logicalEntityName;
        _isEntityEnabled = isEntityEnabled;
        _createProcessor = createProcessor;
        _dispatcher = dispatcher;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_isEntityEnabled())
        {
            LogConsumerNotStarted(_entityKind.ToLogName(), _logicalEntityName);

            return;
        }

        _processor ??= _createProcessor(new ProcessorHandlers(DispatchInNewScopeAsync, HandleProcessorErrorAsync));
        await _processor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is null)
        {
            return;
        }

        await _processor.StopProcessingAsync(cancellationToken);
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (_processor is null)
        {
            return;
        }

        await _processor.DisposeAsync();
        _processor = null;
    }

    private async Task DispatchInNewScopeAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await _dispatcher.DispatchAsync(scope.ServiceProvider, message, cancellationToken);
    }

    private Task HandleProcessorErrorAsync(ProcessErrorEventArgs args)
    {
        LogProcessorError(
            args.Exception,
            _entityKind.ToLogName(),
            _logicalEntityName,
            args.EntityPath,
            args.ErrorSource
        );

        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message =
            "Consumer of {EntityKind} '{LogicalEntityName}' was not started because the entity is disabled"
    )]
    private partial void LogConsumerNotStarted(string entityKind, string logicalEntityName);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message =
            "Service Bus processor error for {EntityKind} '{LogicalEntityName}' (entity name '{EntityName}', source {ErrorSource})"
    )]
    private partial void LogProcessorError(
        Exception exception,
        string entityKind,
        string logicalEntityName,
        string entityName,
        ServiceBusErrorSource errorSource
    );
}
