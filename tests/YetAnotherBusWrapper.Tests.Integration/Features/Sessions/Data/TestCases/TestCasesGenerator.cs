namespace YetAnotherBusWrapper.Tests.Integration.Features.Sessions.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<ISessionsTestCase> Generate()
    {
        yield return new T01TwoSessionsOnQueue();
        yield return new T02OneConcurrentSession();
        yield return new T03TwoSessionsOnSubscription();
        yield return new T04QueueCreatedOnStartup();
        yield return new T05SubscriptionCreatedOnStartup();
    }
}
