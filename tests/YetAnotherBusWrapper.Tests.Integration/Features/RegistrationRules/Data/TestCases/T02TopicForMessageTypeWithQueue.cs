using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T02TopicForMessageTypeWithQueue : IRegistrationRulesTestCase
{
    public int TestCaseId => 2;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddQueue<OrderPlaced>("orders");
        builder.AddTopic<OrderPlaced>("order-events");
    }
}
