using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.HealthChecks.Data.TestCases;

internal class T05DisabledQueueAndTopic : IHealthChecksTestCase
{
    public int TestCaseId => 5;

    public bool EntitiesExist => false;

    public void ConfigureQueue(QueueOptions options)
    {
        options.Enabled = false;
        options.ConnectionString = "";
    }

    public void ConfigureTopic(TopicOptions options)
    {
        options.Enabled = false;
        options.ConnectionString = "";
    }
}
