using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle.Data.TestCases;

internal class T06HostedServicesDisabled : ILifecycleTestCase
{
    public int TestCaseId => 6;

    public void Configure(YetAnotherBusWrapperBuilder builder)
    {
        builder.DisableHostedServices();
    }

    public async Task RunAsync(LifecycleDriver driver, CancellationToken cancellationToken)
    {
        await driver.StartHostAsync(cancellationToken);
        await driver.SendThroughSenderAsync(new OrderPlaced("order-001", 1, OrderStatus.New), cancellationToken);
        await driver.StopHostAsync(cancellationToken);
    }
}
