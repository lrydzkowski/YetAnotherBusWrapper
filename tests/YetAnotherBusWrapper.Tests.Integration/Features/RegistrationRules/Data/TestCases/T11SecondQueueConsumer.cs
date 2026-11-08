using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T11SecondQueueConsumer : IRegistrationRulesTestCase
{
    public int TestCaseId => 11;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddQueue<OrderPlaced>("orders");
        builder.AddQueueConsumer<BillingConsumer, OrderPlaced>();
        builder.AddQueueConsumer<ShippingConsumer, OrderPlaced>();
    }
}
