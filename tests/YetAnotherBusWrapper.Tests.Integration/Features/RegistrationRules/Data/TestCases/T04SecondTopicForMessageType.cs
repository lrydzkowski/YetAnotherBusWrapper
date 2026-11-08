using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T04SecondTopicForMessageType : IRegistrationRulesTestCase
{
    public int TestCaseId => 4;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddTopic<OrderPlaced>("order-events");
        builder.AddTopic<OrderPlaced>("order-events-copy");
    }
}
