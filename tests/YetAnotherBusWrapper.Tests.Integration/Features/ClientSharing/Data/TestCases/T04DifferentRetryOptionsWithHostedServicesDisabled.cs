using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ClientSharing.Data.TestCases;

internal class T04DifferentRetryOptionsWithHostedServicesDisabled : IClientSharingTestCase
{
    public int TestCaseId => 4;

    public int SubscriptionCount => 1;

    public void Configure(YetAnotherBusWrapperBuilder builder)
    {
        builder.DisableHostedServices();
    }

    public void ConfigureTopic(TopicOptions options)
    {
        options.ClientRetry.MaxRetries = 5;
    }
}
