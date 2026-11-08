using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ClientSharing.Data.TestCases;

internal class T03DifferentRetryOptions : IClientSharingTestCase
{
    public int TestCaseId => 3;

    public int SubscriptionCount => 1;

    public void ConfigureTopic(TopicOptions options)
    {
        options.ClientRetry.MaxRetries = 5;
    }
}
