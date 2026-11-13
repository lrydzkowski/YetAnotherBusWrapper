namespace YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<ILifecycleTestCase> Generate()
    {
        yield return new T01StartedAndStoppedOnce();
        yield return new T02StartedAndStoppedTwice();
        yield return new T03StartedConcurrently();
        yield return new T04RestartedAfterStop();
        yield return new T05DisposedWithoutStop();
        yield return new T06HostedServicesDisabled();
        yield return new T07ServicesDisposedSynchronously();
        yield return new T08StartupRollbackFails();
    }
}
