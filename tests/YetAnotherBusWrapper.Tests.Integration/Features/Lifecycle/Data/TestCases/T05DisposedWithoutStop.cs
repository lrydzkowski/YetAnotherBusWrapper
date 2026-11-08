using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle.Data.TestCases;

internal class T05DisposedWithoutStop : ILifecycleTestCase
{
    public int TestCaseId => 5;

    public async Task RunAsync(LifecycleDriver driver, CancellationToken cancellationToken)
    {
        await driver.StartHostAsync(cancellationToken);
        await driver.DisposeHostAsync();
        await driver.SendDirectlyAsync(new OrderPlaced("order-001", 1, OrderStatus.New), cancellationToken);
    }
}
