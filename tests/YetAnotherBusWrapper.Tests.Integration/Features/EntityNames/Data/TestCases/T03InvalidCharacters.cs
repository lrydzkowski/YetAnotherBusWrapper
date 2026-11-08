namespace YetAnotherBusWrapper.Tests.Integration.Features.EntityNames.Data.TestCases;

internal class T03InvalidCharacters : IEntityNamesTestCase
{
    public int TestCaseId => 3;

    public string QueueName => "orders_v1.new/eu";

    public string InstanceName => "Instance_02.West";

    public bool UseInstanceSuffix => true;
}
