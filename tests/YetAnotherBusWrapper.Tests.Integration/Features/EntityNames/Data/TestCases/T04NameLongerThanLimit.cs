namespace YetAnotherBusWrapper.Tests.Integration.Features.EntityNames.Data.TestCases;

internal class T04NameLongerThanLimit : IEntityNamesTestCase
{
    public int TestCaseId => 4;

    public string QueueName => new('a', 300);

    public string InstanceName => "instance-01";

    public bool UseInstanceSuffix => false;
}
