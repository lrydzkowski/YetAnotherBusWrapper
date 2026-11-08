namespace YetAnotherBusWrapper.Tests.Integration.Features.DisabledEntities.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IDisabledEntitiesTestCase> Generate()
    {
        yield return new T01DisabledQueue();
        yield return new T02DisabledTopic();
        yield return new T03DisabledSubscription();
    }
}
