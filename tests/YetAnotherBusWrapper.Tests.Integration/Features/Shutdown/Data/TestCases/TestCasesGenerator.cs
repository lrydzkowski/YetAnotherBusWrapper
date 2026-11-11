namespace YetAnotherBusWrapper.Tests.Integration.Features.Shutdown.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IShutdownTestCase> Generate()
    {
        yield return new T01StopTimeoutElapsedWhileConsumerIsBusy();
    }
}
