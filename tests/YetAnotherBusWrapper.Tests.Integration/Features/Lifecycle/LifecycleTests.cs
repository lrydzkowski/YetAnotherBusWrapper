using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;
using YetAnotherBusWrapper.Tests.Integration.Common.Host;
using YetAnotherBusWrapper.Tests.Integration.Common.Logging;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;
using YetAnotherBusWrapper.Tests.Integration.Common.Services;
using YetAnotherBusWrapper.Tests.Integration.Common.TestCollections;
using YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle;

[Collection(EmulatorTestCollection3.CollectionName)]
public class LifecycleTests
{
    private const string Feature = "lifecycle";
    private const string LogicalEntityName = "orders";

    private static readonly TimeSpan DeliveryObservationPeriod = TimeSpan.FromSeconds(5);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public LifecycleTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
        _verifySettings.ScrubMember<MessageContext>(context => context.EnqueuedTime);
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task Lifecycle_ShouldStartAndStopConsumersOnce(ILifecycleTestCase testCase)
    {
        LifecycleTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<LifecycleTestResult> RunAsync(ILifecycleTestCase testCase, CancellationToken cancellationToken)
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string queueName = await scope.CreateQueueAsync(LogicalEntityName, cancellationToken);
        DeliveryRecorder<OrderPlaced> recorder = new();
        LifecycleDriver? driver = null;

        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, queueName, recorder);
            driver = new LifecycleDriver(host, scope, queueName);
            await testCase.RunAsync(driver, cancellationToken);
            await recorder.WaitForDeliveriesAsync(
                driver.SentMessageCount,
                DeliveryObservationPeriod,
                cancellationToken
            );
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        List<PeekedMessageDto> remainingMessages = await scope.Inspector.PeekQueueAsync(queueName, cancellationToken);

        return new LifecycleTestResult(
            testCase.TestCaseId,
            driver?.GetSendOutcomes() ?? [],
            recorder.GetDeliveries(),
            remainingMessages,
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static IHost BuildHost(
        ILifecycleTestCase testCase,
        TestCaseScope scope,
        string queueName,
        DeliveryRecorder<OrderPlaced> recorder
    )
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services =>
            {
                services.AddSingleton(recorder);
                services.AddYetAnotherBusWrapper(
                    builder =>
                    {
                        builder.AddQueue<OrderPlaced>(LogicalEntityName)
                            .Configure(
                                options =>
                                {
                                    options.ConnectionString = scope.ConnectionString;
                                    options.AdministrationConnectionString = scope.AdministrationConnectionString;
                                    options.QueueName = queueName;
                                    options.CreateOnStartup = true;
                                }
                            );
                        builder.AddQueueConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>();
                        testCase.Configure(builder);
                    }
                );
            }
        );
    }
}

internal record LifecycleTestResult(
    int TestCaseId,
    List<SendOutcomeDto> SendOutcomes,
    List<RecordedDelivery<OrderPlaced>> Deliveries,
    List<PeekedMessageDto> RemainingMessages,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
