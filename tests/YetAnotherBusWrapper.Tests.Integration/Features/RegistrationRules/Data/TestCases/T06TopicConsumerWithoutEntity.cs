using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T06TopicConsumerWithoutEntity : IRegistrationRulesTestCase
{
    public int TestCaseId => 6;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddTopicConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>("billing");
    }
}
