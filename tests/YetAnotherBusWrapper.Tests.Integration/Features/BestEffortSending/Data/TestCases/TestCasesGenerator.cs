namespace YetAnotherBusWrapper.Tests.Integration.Features.BestEffortSending.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IBestEffortSendingTestCase> Generate()
    {
        yield return new T01EnabledQueue();
        yield return new T02DisabledQueue();
        yield return new T03DisabledTopic();
        yield return new T04MissingQueue();
        yield return new T05OversizedMessageAfterFirstBatch();
    }
}
