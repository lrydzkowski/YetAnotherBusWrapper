using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T10ValidRegistrations : IRegistrationRulesTestCase
{
    public int TestCaseId => 10;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddQueue<ReportGenerated>("reports");
        builder.AddQueueConsumer<RecordingConsumer<ReportGenerated>, ReportGenerated>();
        builder.AddTopic<OrderPlaced>("order-events");
        builder.AddTopicConsumer<BillingConsumer, OrderPlaced>("billing");
        builder.AddTopicConsumer<ShippingConsumer, OrderPlaced>("shipping");
    }
}
