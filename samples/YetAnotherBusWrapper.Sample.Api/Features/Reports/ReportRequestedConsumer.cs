using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Sample.Api.Features.Received;

namespace YetAnotherBusWrapper.Sample.Api.Features.Reports;

public sealed partial class ReportRequestedConsumer : IMessageConsumer<ReportRequested>
{
    private readonly ILogger<ReportRequestedConsumer> _logger;
    private readonly ReceivedMessageLog _receivedMessageLog;

    public ReportRequestedConsumer(ReceivedMessageLog receivedMessageLog, ILogger<ReportRequestedConsumer> logger)
    {
        _receivedMessageLog = receivedMessageLog;
        _logger = logger;
    }

    public Task ConsumeAsync(ReportRequested message, MessageContext context, CancellationToken cancellationToken)
    {
        LogReportRequested(message.ReportId, message.Name, context.EnqueuedTime);
        _receivedMessageLog.Record(nameof(ReportRequestedConsumer), message, context);

        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Report '{ReportId}' ({Name}) requested, enqueued at {EnqueuedTime}"
    )]
    private partial void LogReportRequested(string reportId, string name, DateTimeOffset enqueuedTime);
}
