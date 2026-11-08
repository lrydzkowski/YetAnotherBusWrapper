using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T07QueueConsumerForTopic : IRegistrationRulesTestCase
{
    public int TestCaseId => 7;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddTopic<OrderPlaced>("order-events");
        builder.AddQueueConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>();
    }
}
