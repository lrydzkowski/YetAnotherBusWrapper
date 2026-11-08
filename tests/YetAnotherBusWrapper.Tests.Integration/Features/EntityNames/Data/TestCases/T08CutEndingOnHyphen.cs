namespace YetAnotherBusWrapper.Tests.Integration.Features.EntityNames.Data.TestCases;

internal class T08CutEndingOnHyphen : IEntityNamesTestCase
{
    public int TestCaseId => 8;

    public string QueueName => $"{new string('a', 229)}-{new string('b', 50)}";

    public string InstanceName => "instance-01";

    public bool UseInstanceSuffix => true;
}
