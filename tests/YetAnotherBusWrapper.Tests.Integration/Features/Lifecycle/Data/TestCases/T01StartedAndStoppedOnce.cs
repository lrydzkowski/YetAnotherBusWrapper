using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle.Data.TestCases;

internal class T01StartedAndStoppedOnce : ILifecycleTestCase
{
    public int TestCaseId => 1;

    public async Task RunAsync(LifecycleDriver driver, CancellationToken cancellationToken)
    {
        await driver.StartHostAsync(cancellationToken);
        await driver.StopHostAsync(cancellationToken);
        await driver.SendThroughSenderAsync(new OrderPlaced("order-001", 1, OrderStatus.New), cancellationToken);
    }
}
