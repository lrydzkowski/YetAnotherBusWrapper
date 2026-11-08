using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle.Data.TestCases;

internal class T03StartedConcurrently : ILifecycleTestCase
{
    public int TestCaseId => 3;

    public async Task RunAsync(LifecycleDriver driver, CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            driver.StartHostedServicesAsync(cancellationToken),
            driver.StartHostedServicesAsync(cancellationToken),
            driver.StartHostedServicesAsync(cancellationToken)
        );
        await driver.StopHostedServicesAsync(cancellationToken);
        await driver.SendThroughSenderAsync(new OrderPlaced("order-001", 1, OrderStatus.New), cancellationToken);
    }
}
