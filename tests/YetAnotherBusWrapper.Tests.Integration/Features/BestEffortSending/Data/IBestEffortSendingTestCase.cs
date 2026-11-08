using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.BestEffortSending.Data;

public interface IBestEffortSendingTestCase
{
    int TestCaseId { get; }
    bool QueueExists => true;
    IReadOnlyList<ReportGenerated> QueueMessages => [];
    IReadOnlyList<OrderPlaced> TopicMessages => [];

    void ConfigureQueue(QueueOptions options)
    {
    }

    void ConfigureTopic(TopicOptions options)
    {
    }
}
