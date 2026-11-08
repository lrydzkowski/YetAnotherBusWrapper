namespace YetAnotherBusWrapper.Tests.Integration.Features.EntityNames.Data;

public interface IEntityNamesTestCase
{
    int TestCaseId { get; }
    string QueueName { get; }
    string InstanceName { get; }
    bool UseInstanceSuffix { get; }
}
