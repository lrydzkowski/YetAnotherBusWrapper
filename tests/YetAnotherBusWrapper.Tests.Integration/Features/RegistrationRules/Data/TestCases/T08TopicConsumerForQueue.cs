using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T08TopicConsumerForQueue : IRegistrationRulesTestCase
{
    public int TestCaseId => 8;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddQueue<OrderPlaced>("orders");
        builder.AddTopicConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>("billing");
    }
}
