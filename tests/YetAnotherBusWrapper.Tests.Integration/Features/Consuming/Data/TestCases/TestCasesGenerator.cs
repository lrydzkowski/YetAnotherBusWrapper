namespace YetAnotherBusWrapper.Tests.Integration.Features.Consuming.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IConsumingTestCase> Generate()
    {
        yield return new T01SingleMessage();
        yield return new T02SeveralMessagesSentOneByOne();
        yield return new T03MessageSentAfterHostStops();
    }
}
