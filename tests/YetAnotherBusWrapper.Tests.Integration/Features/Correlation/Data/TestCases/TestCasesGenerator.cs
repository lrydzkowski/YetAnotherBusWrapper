namespace YetAnotherBusWrapper.Tests.Integration.Features.Correlation.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<ICorrelationTestCase> Generate()
    {
        yield return new T01WithoutMessageCorrelationId();
        yield return new T02WithMessageCorrelationId();
        yield return new T03ConsumerFailsWithMessageCorrelationId();
    }
}
