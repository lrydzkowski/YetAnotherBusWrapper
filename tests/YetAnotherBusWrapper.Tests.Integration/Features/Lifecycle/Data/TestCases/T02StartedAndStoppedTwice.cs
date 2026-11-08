using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle.Data.TestCases;

internal class T02StartedAndStoppedTwice : ILifecycleTestCase
{
    public int TestCaseId => 2;

    public async Task RunAsync(LifecycleDriver driver, CancellationToken cancellationToken)
    {
        await driver.StartHostAsync(cancellationToken);
        await driver.StartHostedServicesAsync(cancellationToken);
        await driver.StopHostAsync(cancellationToken);
        await driver.StopHostedServicesAsync(cancellationToken);
        await driver.SendThroughSenderAsync(new OrderPlaced("order-001", 1, OrderStatus.New), cancellationToken);
    }
}
