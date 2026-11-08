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
using YetAnotherBusWrapper.Tests.Integration.Features.Consuming.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.Consuming.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Consuming;

[Collection(EmulatorTestCollection4.CollectionName)]
public class ConsumingTests
{
    private const string Feature = "consuming";
    private const string LogicalEntityName = "orders";

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan AfterStopObservationPeriod = TimeSpan.FromSeconds(5);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public ConsumingTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task Consuming_ShouldDeliverSentMessagesToConsumer(IConsumingTestCase testCase)
    {
        ConsumingTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<ConsumingTestResult> RunAsync(IConsumingTestCase testCase, CancellationToken cancellationToken)
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string queueName = await scope.CreateQueueAsync(LogicalEntityName, cancellationToken);
        DeliveryRecorder<OrderPlaced> recorder = new();
        List<SendResult> sendResults = [];

        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(scope, queueName, recorder);
            await host.StartAsync(cancellationToken);
            sendResults.AddRange(await SendAsync(host, testCase.Messages, cancellationToken));
            await recorder.WaitForDeliveriesAsync(testCase.Messages.Count, DeliveryTimeout, cancellationToken);
            await host.StopAsync(cancellationToken);
            sendResults.AddRange(await SendAsync(host, testCase.MessagesSentAfterStop, cancellationToken));
            await WaitForDeliveriesAfterStopAsync(testCase, recorder, cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        List<PeekedMessageDto> remainingMessages = await scope.Inspector.PeekQueueAsync(queueName, cancellationToken);

        return new ConsumingTestResult(
            testCase.TestCaseId,
            testCase.Messages,
            testCase.MessagesSentAfterStop,
            sendResults,
            recorder.GetDeliveries(),
            remainingMessages,
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static IHost BuildHost(TestCaseScope scope, string queueName, DeliveryRecorder<OrderPlaced> recorder)
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
                                    options.QueueName = queueName;
                                }
                            );
                        builder.AddQueueConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>();
                    }
                );
            }
        );
    }

    private static async Task<List<SendResult>> SendAsync(
        IHost host,
        IReadOnlyList<OrderPlaced> messages,
        CancellationToken cancellationToken
    )
    {
        IMessageSender<OrderPlaced> sender = host.Services.GetRequiredService<IMessageSender<OrderPlaced>>();
        List<SendResult> sendResults = [];
        foreach (OrderPlaced message in messages)
        {
            sendResults.Add(await sender.SendAsync(message, cancellationToken: cancellationToken));
        }

        return sendResults;
    }

    private static async Task WaitForDeliveriesAfterStopAsync(
        IConsumingTestCase testCase,
        DeliveryRecorder<OrderPlaced> recorder,
        CancellationToken cancellationToken
    )
    {
        if (testCase.MessagesSentAfterStop.Count == 0)
        {
            return;
        }

        int allMessagesCount = testCase.Messages.Count + testCase.MessagesSentAfterStop.Count;
        await recorder.WaitForDeliveriesAsync(allMessagesCount, AfterStopObservationPeriod, cancellationToken);
    }
}

internal record ConsumingTestResult(
    int TestCaseId,
    IReadOnlyList<OrderPlaced> SentMessages,
    IReadOnlyList<OrderPlaced> MessagesSentAfterStop,
    List<SendResult> SendResults,
    List<RecordedDelivery<OrderPlaced>> Deliveries,
    List<PeekedMessageDto> RemainingMessages,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
