using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using YetAnotherBusWrapper.Consuming;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle.Data.TestCases;

internal class T08StartupRollbackFails : ILifecycleTestCase
{
    public int TestCaseId => 8;

    public void Configure(YetAnotherBusWrapperBuilder builder)
    {
        IConsumerProcessor processor = Substitute.For<IConsumerProcessor>();
        processor.StartAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Consumer failed to start.")));
        processor.StopAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Consumer failed to stop.")));
        builder.Services.AddSingleton(processor);
    }

    public async Task RunAsync(LifecycleDriver driver, CancellationToken cancellationToken)
    {
        await driver.StartHostAsync(cancellationToken);
    }
}
