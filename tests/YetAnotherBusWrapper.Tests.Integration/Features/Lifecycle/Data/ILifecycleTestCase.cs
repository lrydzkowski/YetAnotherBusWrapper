namespace YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle.Data;

public interface ILifecycleTestCase
{
    int TestCaseId { get; }

    void Configure(YetAnotherBusWrapperBuilder builder)
    {
    }

    Task RunAsync(LifecycleDriver driver, CancellationToken cancellationToken);
}
