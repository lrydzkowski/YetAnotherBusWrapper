using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.BestEffortSending.Data.TestCases;

internal class T03DisabledTopic : IBestEffortSendingTestCase
{
    public int TestCaseId => 3;

    public IReadOnlyList<OrderPlaced> TopicMessages => [new("order-001", 2, OrderStatus.New)];

    public void ConfigureTopic(TopicOptions options)
    {
        options.Enabled = false;
        options.ConnectionString = "";
    }
}
