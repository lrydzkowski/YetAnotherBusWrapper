using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ClientSharing.Data.TestCases;

internal class T02SameCustomRetryOptions : IClientSharingTestCase
{
    public int TestCaseId => 2;

    public int SubscriptionCount => 1;

    public void ConfigureQueue(QueueOptions options)
    {
        options.ClientRetry.MaxRetries = 4;
        options.ClientRetry.TryTimeout = TimeSpan.FromSeconds(20);
        options.ClientRetry.MaxDelay = TimeSpan.FromSeconds(5);
    }

    public void ConfigureTopic(TopicOptions options)
    {
        options.ClientRetry.MaxRetries = 4;
        options.ClientRetry.TryTimeout = TimeSpan.FromSeconds(20);
        options.ClientRetry.MaxDelay = TimeSpan.FromSeconds(5);
    }
}
