using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T09DisabledQueueWithInvalidValues : IOptionsValidationTestCase
{
    public int TestCaseId => 9;

    public void Configure(YetAnotherBusWrapperBuilder builder, TestCaseScope scope)
    {
        builder.AddQueue<OrderPlaced>("orders")
            .Configure(
                options =>
                {
                    options.Enabled = false;
                    options.MaxDeliveryCount = 0;
                    options.DuplicateDetectionWindow = TimeSpan.FromSeconds(-1);
                    options.ClientRetry.MaxRetries = -1;
                }
            );
        builder.AddQueueConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>()
            .Configure(options => options.MaxConcurrentCalls = 0);
    }
}
