namespace YetAnotherBusWrapper.Tests.Integration.Features.EntityNames.Data.TestCases;

internal class T01WithoutInstanceSuffix : IEntityNamesTestCase
{
    public int TestCaseId => 1;

    public string QueueName => "orders";

    public string InstanceName => "instance-01";

    public bool UseInstanceSuffix => false;
}
