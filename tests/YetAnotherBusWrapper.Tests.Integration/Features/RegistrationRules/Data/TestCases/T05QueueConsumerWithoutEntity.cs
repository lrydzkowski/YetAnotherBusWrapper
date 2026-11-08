using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T05QueueConsumerWithoutEntity : IRegistrationRulesTestCase
{
    public int TestCaseId => 5;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddQueueConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>();
    }
}
