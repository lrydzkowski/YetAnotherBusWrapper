using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T06QueueConsumerWithInvalidValues : IOptionsValidationTestCase
{
    public int TestCaseId => 6;

    public void Configure(YetAnotherBusWrapperBuilder builder, TestCaseScope scope)
    {
        builder.AddQueue<OrderPlaced>("orders")
            .Configure(
                options =>
                {
                    options.ConnectionString = scope.ConnectionString;
                    options.QueueName = scope.GetQueueName("orders");
                }
            );
        builder.AddQueueConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>()
            .Configure(
                options =>
                {
                    options.MaxConcurrentCalls = 0;
                    options.PrefetchCount = -1;
                    options.MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(-1);
                    options.MaxConcurrentSessions = 0;
                    options.SessionIdleTimeout = TimeSpan.Zero;
                }
            );
    }
}
