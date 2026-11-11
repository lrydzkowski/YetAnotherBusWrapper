namespace YetAnotherBusWrapper.Tests.Integration.Features.Shutdown.Data.TestCases;

internal class T01StopTimeoutElapsedWhileConsumerIsBusy : IShutdownTestCase
{
    public int TestCaseId => 1;

    public TimeSpan ConsumerBusyDuration => TimeSpan.FromSeconds(5);

    public TimeSpan StopTimeout => TimeSpan.FromSeconds(1);
}
