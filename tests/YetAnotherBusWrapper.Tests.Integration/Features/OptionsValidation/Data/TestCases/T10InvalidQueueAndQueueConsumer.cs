using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T10InvalidQueueAndQueueConsumer : IOptionsValidationTestCase
{
    public int TestCaseId => 10;

    public void Configure(YetAnotherBusWrapperBuilder builder, TestCaseScope scope)
    {
        builder.AddQueue<OrderPlaced>("orders")
            .Configure(
                options =>
                {
                    options.ConnectionString = scope.ConnectionString;
                    options.QueueName = scope.GetQueueName("orders");
                    options.MaxDeliveryCount = 0;
                }
            );
        builder.AddQueueConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>()
            .Configure(options => options.MaxConcurrentCalls = 0);
    }
}
