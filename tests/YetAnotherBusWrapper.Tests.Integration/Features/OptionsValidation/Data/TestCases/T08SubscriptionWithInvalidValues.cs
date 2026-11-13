using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T08SubscriptionWithInvalidValues : IOptionsValidationTestCase
{
    public int TestCaseId => 8;

    public void Configure(YetAnotherBusWrapperBuilder builder, TestCaseScope scope)
    {
        builder.AddTopic<OrderPlaced>("orders")
            .Configure(
                options =>
                {
                    options.ConnectionString = scope.ConnectionString;
                    options.TopicName = scope.GetTopicName("orders");
                }
            );
        builder.AddTopicConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>("billing")
            .Configure(
                options =>
                {
                    options.SubscriptionName = "billing";
                    options.MaxDeliveryCount = 0;
                    options.MaxConcurrentCalls = 0;
                    options.PrefetchCount = -1;
                    options.MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(-1);
                    options.MaxConcurrentSessions = 0;
                    options.SessionIdleTimeout = TimeSpan.FromSeconds(-1);
                }
            );
    }
}
