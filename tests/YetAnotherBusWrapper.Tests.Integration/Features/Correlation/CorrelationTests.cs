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
using YetAnotherBusWrapper.Tests.Integration.Features.Correlation.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.Correlation.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Correlation;

[Collection(EmulatorTestCollection1.CollectionName)]
public class CorrelationTests
{
    private const string Feature = "correlation";
    private const string LogicalEntityName = "orders";
    private const string MessageSenderLogger = "MessageSender";

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(30);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public CorrelationTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task Correlation_ShouldHandleEachMessageInLoggingScopeWithCorrelationId(
        ICorrelationTestCase testCase
    )
    {
        CorrelationTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<CorrelationTestResult> RunAsync(
        ICorrelationTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string queueName = await scope.CreateQueueAsync(LogicalEntityName, cancellationToken);
        DeliveryRecorder<OrderPlaced> recorder = new();

        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, queueName, recorder);
            await host.StartAsync(cancellationToken);
            await SendAsync(host, testCase.Messages, cancellationToken);
            await recorder.WaitForDeliveriesAsync(testCase.DeliveriesToWaitFor, DeliveryTimeout, cancellationToken);
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        List<PeekedMessageDto> remainingMessages = await scope.Inspector.PeekQueueAsync(queueName, cancellationToken);

        return new CorrelationTestResult(
            testCase.TestCaseId,
            testCase.FailurePlan,
            testCase.Messages,
            recorder.GetDeliveries(),
            remainingMessages,
            scope.Logs.GetLogsWithScopes().Where(log => log.Logger != MessageSenderLogger).ToList(),
            thrownException
        );
    }

    private static IHost BuildHost(
        ICorrelationTestCase testCase,
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
                services.AddSingleton(testCase.FailurePlan);
                services.AddYetAnotherBusWrapper(
                    builder =>
                    {
                        builder.AddQueue(LogicalEntityName, configureMessage: testCase.ConfigureMessage)
                            .Configure(
                                options =>
                                {
                                    options.ConnectionString = scope.ConnectionString;
                                    options.QueueName = queueName;
                                }
                            );
                        builder.AddQueueConsumer<FailingConsumer, OrderPlaced>();
                    }
                );
            }
        );
    }

    private static async Task SendAsync(
        IHost host,
        IReadOnlyList<OrderPlaced> messages,
        CancellationToken cancellationToken
    )
    {
        IMessageSender<OrderPlaced> sender = host.Services.GetRequiredService<IMessageSender<OrderPlaced>>();
        foreach (OrderPlaced message in messages)
        {
            await sender.SendAsync(message, cancellationToken: cancellationToken);
        }
    }
}

internal record CorrelationTestResult(
    int TestCaseId,
    ConsumerFailurePlan FailurePlan,
    IReadOnlyList<OrderPlaced> SentMessages,
    List<RecordedDelivery<OrderPlaced>> Deliveries,
    List<PeekedMessageDto> RemainingMessages,
    List<CapturedScopedLogDto> Logs,
    ThrownException? ThrownException
);
