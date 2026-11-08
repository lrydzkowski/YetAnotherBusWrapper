namespace YetAnotherBusWrapper.Tests.Integration.Features.RetriesAndDeadLettering.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IRetriesAndDeadLetteringTestCase> Generate()
    {
        yield return new T01ConsumerAlwaysThrows();
        yield return new T02ConsumerThrowsOnce();
        yield return new T03BodyIsNotValidJson();
        yield return new T04BodyDeserializesToNull();
    }
}
