namespace YetAnotherBusWrapper.Tests.Integration.Features.HealthChecks.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IHealthChecksTestCase> Generate()
    {
        yield return new T01ExistingEntities();
        yield return new T02ExistingEntitiesWithPeekCheck();
        yield return new T03MissingEntities();
        yield return new T04MissingEntitiesWithPeekCheck();
        yield return new T05DisabledQueueAndTopic();
        yield return new T06DisabledSubscription();
        yield return new T07SessionEntitiesWithPeekCheck();
    }
}
