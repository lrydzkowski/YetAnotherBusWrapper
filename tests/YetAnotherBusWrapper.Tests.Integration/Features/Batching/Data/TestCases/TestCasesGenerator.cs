namespace YetAnotherBusWrapper.Tests.Integration.Features.Batching.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IBatchingTestCase> Generate()
    {
        yield return new T01SmallCollection();
        yield return new T02CollectionLargerThanBatchLimit();
        yield return new T03OversizedMessageInCollection();
        yield return new T04OversizedSingleMessage();
        yield return new T05EmptyCollection();
    }
}
