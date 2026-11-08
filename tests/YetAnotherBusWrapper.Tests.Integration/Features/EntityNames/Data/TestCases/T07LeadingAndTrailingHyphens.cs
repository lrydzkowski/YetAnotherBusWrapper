namespace YetAnotherBusWrapper.Tests.Integration.Features.EntityNames.Data.TestCases;

internal class T07LeadingAndTrailingHyphens : IEntityNamesTestCase
{
    public int TestCaseId => 7;

    public string QueueName => "orders--";

    public string InstanceName => "-_instance-01-";

    public bool UseInstanceSuffix => true;
}
