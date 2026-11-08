using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ClientSharing.Data;

public interface IClientSharingTestCase
{
    int TestCaseId { get; }
    int SubscriptionCount { get; }

    void Configure(YetAnotherBusWrapperBuilder builder)
    {
    }

    void ConfigureQueue(QueueOptions options)
    {
    }

    void ConfigureTopic(TopicOptions options)
    {
    }
}
