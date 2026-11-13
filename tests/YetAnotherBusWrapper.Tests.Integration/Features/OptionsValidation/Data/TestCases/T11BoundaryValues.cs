using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T11BoundaryValues : IOptionsValidationTestCase
{
    public int TestCaseId => 11;

    public void Configure(YetAnotherBusWrapperBuilder builder, TestCaseScope scope)
    {
        builder.AddQueue<OrderPlaced>("orders")
            .Configure(
                options =>
                {
                    options.ConnectionString = scope.ConnectionString;
                    options.AdministrationConnectionString = scope.AdministrationConnectionString;
                    options.QueueName = scope.GetQueueName("orders");
                    options.CreateOnStartup = true;
                    options.FailStartupOnProvisioningError = true;
                    options.MaxDeliveryCount = 1;
                    options.DuplicateDetectionWindow = TimeSpan.FromSeconds(20);
                    options.ClientRetry = new ClientRetryOptions
                    {
                        MaxRetries = 100,
                        TryTimeout = TimeSpan.FromHours(1),
                        MaxDelay = TimeSpan.Zero
                    };
                }
            );
        builder.AddQueueConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>()
            .Configure(
                options =>
                {
                    options.MaxConcurrentCalls = 1;
                    options.PrefetchCount = 0;
                    options.MaxAutoLockRenewalDuration = Timeout.InfiniteTimeSpan;
                    options.MaxConcurrentSessions = 1;
                    options.SessionIdleTimeout = TimeSpan.FromMilliseconds(1);
                }
            );
    }
}
