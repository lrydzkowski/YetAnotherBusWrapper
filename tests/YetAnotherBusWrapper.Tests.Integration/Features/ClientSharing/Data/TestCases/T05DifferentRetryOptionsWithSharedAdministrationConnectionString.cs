using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ClientSharing.Data.TestCases;

internal class T05DifferentRetryOptionsWithSharedAdministrationConnectionString : IClientSharingTestCase
{
    public int TestCaseId => 5;

    public int SubscriptionCount => 1;

    public void ConfigureTopic(TopicOptions options)
    {
        options.ConnectionString =
            "Endpoint=sb://other-namespace.servicebus.windows.net/;SharedAccessKeyName=name;SharedAccessKey=key";
        options.ClientRetry.MaxRetries = 5;
    }
}
