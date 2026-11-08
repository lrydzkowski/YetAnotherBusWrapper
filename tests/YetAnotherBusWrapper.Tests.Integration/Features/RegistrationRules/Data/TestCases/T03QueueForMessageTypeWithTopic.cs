using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T03QueueForMessageTypeWithTopic : IRegistrationRulesTestCase
{
    public int TestCaseId => 3;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddTopic<OrderPlaced>("order-events");
        builder.AddQueue<OrderPlaced>("orders");
    }
}
