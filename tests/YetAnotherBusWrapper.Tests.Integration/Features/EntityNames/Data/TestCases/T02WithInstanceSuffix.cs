namespace YetAnotherBusWrapper.Tests.Integration.Features.EntityNames.Data.TestCases;

internal class T02WithInstanceSuffix : IEntityNamesTestCase
{
    public int TestCaseId => 2;

    public string QueueName => "orders";

    public string InstanceName => "instance-01";

    public bool UseInstanceSuffix => true;
}
