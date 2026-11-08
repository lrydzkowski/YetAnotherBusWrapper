namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicConsuming.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<ITopicConsumingTestCase> Generate()
    {
        yield return new T01SingleMessage();
        yield return new T02SeveralMessages();
        yield return new T03MessagePropertiesFromContent();
    }
}
