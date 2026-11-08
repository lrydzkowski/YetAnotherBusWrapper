namespace YetAnotherBusWrapper.Tests.Integration.Features.HealthChecks.Data.TestCases;

internal class T03MissingEntities : IHealthChecksTestCase
{
    public int TestCaseId => 3;

    public bool EntitiesExist => false;
}
