using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Tests.Integration.Features.DisabledEntities.Data.TestCases;

internal class T01DisabledQueue : IDisabledEntitiesTestCase
{
    public int TestCaseId => 1;

    public int ExpectedQueueDeliveries => 0;

    public int ExpectedBillingDeliveries => 1;

    public int ExpectedShippingDeliveries => 1;

    public void ConfigureQueue(QueueOptions options)
    {
        options.Enabled = false;
        options.ConnectionString = "";
        options.QueueName = null;
    }
}
