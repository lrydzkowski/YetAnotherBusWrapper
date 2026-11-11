namespace YetAnotherBusWrapper.Tests.Integration.Features.Shutdown.Data.TestCases;

internal class T02ConsumerCancelledOnStop : IShutdownTestCase
{
    public int TestCaseId => 2;

    public TimeSpan ConsumerBusyDuration => TimeSpan.FromSeconds(60);

    public bool ConsumerObservesCancellation => true;

    public TimeSpan StopTimeout => TimeSpan.FromSeconds(30);
}
