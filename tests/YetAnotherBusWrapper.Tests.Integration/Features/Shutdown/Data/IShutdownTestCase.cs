namespace YetAnotherBusWrapper.Tests.Integration.Features.Shutdown.Data;

public interface IShutdownTestCase
{
    int TestCaseId { get; }
    TimeSpan ConsumerBusyDuration { get; }
    bool ConsumerObservesCancellation { get; }
    TimeSpan StopTimeout { get; }
}
