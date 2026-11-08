using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T01SecondQueueForMessageType : IRegistrationRulesTestCase
{
    public int TestCaseId => 1;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddQueue<OrderPlaced>("orders");
        builder.AddQueue<OrderPlaced>("orders-copy");
    }
}
