using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T09SameSubscriptionTwice : IRegistrationRulesTestCase
{
    public int TestCaseId => 9;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddTopic<OrderPlaced>("order-events");
        builder.AddTopicConsumer<BillingConsumer, OrderPlaced>("billing");
        builder.AddTopicConsumer<ShippingConsumer, OrderPlaced>("billing");
    }
}
