using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T05QueueWithInvalidValues : IOptionsValidationTestCase
{
    public int TestCaseId => 5;

    public void Configure(YetAnotherBusWrapperBuilder builder, TestCaseScope scope)
    {
        builder.AddQueue<OrderPlaced>("orders")
            .Configure(
                options =>
                {
                    options.ConnectionString = scope.ConnectionString;
                    options.QueueName = scope.GetQueueName("orders");
                    options.MaxDeliveryCount = 0;
                    options.DuplicateDetectionWindow = TimeSpan.FromSeconds(19);
                    options.ClientRetry = new ClientRetryOptions
                    {
                        MaxRetries = -1,
                        TryTimeout = TimeSpan.FromMinutes(61),
                        MaxDelay = TimeSpan.FromSeconds(-1)
                    };
                }
            );
    }
}
