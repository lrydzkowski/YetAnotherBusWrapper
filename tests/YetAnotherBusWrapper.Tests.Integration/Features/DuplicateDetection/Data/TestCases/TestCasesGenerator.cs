namespace YetAnotherBusWrapper.Tests.Integration.Features.DuplicateDetection.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IDuplicateDetectionTestCase> Generate()
    {
        yield return new T01SameMessageIdInSeparateSends();
        yield return new T02DifferentMessageIds();
        yield return new T03SameMessageIdInOneCollection();
        yield return new T04DuplicateDetectionOff();
    }
}
