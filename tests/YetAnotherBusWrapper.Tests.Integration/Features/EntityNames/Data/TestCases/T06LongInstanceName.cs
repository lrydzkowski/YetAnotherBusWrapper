namespace YetAnotherBusWrapper.Tests.Integration.Features.EntityNames.Data.TestCases;

internal class T06LongInstanceName : IEntityNamesTestCase
{
    public int TestCaseId => 6;

    public string QueueName => new('a', 250);

    public string InstanceName => "9f2c4e6a8b0d1f3e5a7c9b1d3f5e7a9c0b2d4f6e8a1c3e5b7d9f0a2c4e6b8d1f";

    public bool UseInstanceSuffix => true;
}
