using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T07TopicWithInvalidValues : IOptionsValidationTestCase
{
    public int TestCaseId => 7;

    public void Configure(YetAnotherBusWrapperBuilder builder, TestCaseScope scope)
    {
        builder.AddTopic<OrderPlaced>("orders")
            .Configure(
                options =>
                {
                    options.ConnectionString = scope.ConnectionString;
                    options.TopicName = scope.GetTopicName("orders");
                    options.DuplicateDetectionWindow = TimeSpan.FromDays(8);
                    options.ClientRetry = new ClientRetryOptions
                    {
                        MaxRetries = 101,
                        TryTimeout = TimeSpan.FromSeconds(-1),
                        MaxDelay = TimeSpan.FromSeconds(-1)
                    };
                }
            );
    }
}
