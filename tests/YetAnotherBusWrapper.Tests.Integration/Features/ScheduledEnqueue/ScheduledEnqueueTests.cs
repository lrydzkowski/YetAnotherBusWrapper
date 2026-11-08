using Azure.Messaging.ServiceBus;
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
using YetAnotherBusWrapper.Tests.Integration.Features.ScheduledEnqueue.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.ScheduledEnqueue.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ScheduledEnqueue;

[Collection(EmulatorTestCollection4.CollectionName)]
public class ScheduledEnqueueTests
{
    private const string Feature = "scheduled-enqueue";
    private const string LogicalEntityName = "orders";

    private static readonly TimeSpan ScheduleDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan EarlyDeliveryMargin = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(30);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public ScheduledEnqueueTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
        _verifySettings.ScrubMember<MessageContext>(context => context.EnqueuedTime);
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task ScheduledEnqueue_ShouldNotDeliverMessagesBeforeScheduledTime(IScheduledEnqueueTestCase testCase)
    {
        ScheduledEnqueueTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<ScheduledEnqueueTestResult> RunAsync(
        IScheduledEnqueueTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string queueName = await scope.CreateQueueAsync(LogicalEntityName, cancellationToken);
        DeliveryRecorder<OrderPlaced> recorder = new();

        SendResult? sendResult = null;
        List<ScheduledMessageDto> scheduledMessages = [];
        int? deliveriesBeforeScheduledTime = null;
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(scope, queueName, recorder);
            await host.StartAsync(cancellationToken);
            DateTimeOffset scheduledEnqueueTime = TimeProvider.System.GetUtcNow().Add(ScheduleDelay);
            IMessageSender<OrderPlaced> sender = host.Services.GetRequiredService<IMessageSender<OrderPlaced>>();
            sendResult = await sender.SendAsync(
                testCase.Messages,
                new SendOptions { ScheduledEnqueueTime = scheduledEnqueueTime },
                cancellationToken
            );
            scheduledMessages = await PeekScheduledMessagesAsync(
                scope,
                queueName,
                scheduledEnqueueTime,
                cancellationToken
            );
            await WaitUntilAsync(scheduledEnqueueTime - EarlyDeliveryMargin, cancellationToken);
            deliveriesBeforeScheduledTime = recorder.GetDeliveries().Count;
            await recorder.WaitForDeliveriesAsync(testCase.Messages.Count, DeliveryTimeout, cancellationToken);
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        return new ScheduledEnqueueTestResult(
            testCase.TestCaseId,
            testCase.Messages,
            sendResult,
            scheduledMessages,
            deliveriesBeforeScheduledTime,
            recorder.GetDeliveries(),
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

    private static async Task<List<ScheduledMessageDto>> PeekScheduledMessagesAsync(
        TestCaseScope scope,
        string queueName,
        DateTimeOffset scheduledEnqueueTime,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ServiceBusReceivedMessage> messages = await scope.Inspector.PeekQueueMessagesAsync(
            queueName,
            cancellationToken
        );

        return messages.Select(message => ScheduledMessageDto.From(message, scheduledEnqueueTime)).ToList();
    }

    private static async Task WaitUntilAsync(DateTimeOffset time, CancellationToken cancellationToken)
    {
        TimeSpan delay = time - TimeProvider.System.GetUtcNow();
        if (delay <= TimeSpan.Zero)
        {
            return;
        }

        await Task.Delay(delay, cancellationToken);
    }
}

internal record ScheduledEnqueueTestResult(
    int TestCaseId,
    IReadOnlyList<OrderPlaced> SentMessages,
    SendResult? SendResult,
    List<ScheduledMessageDto> ScheduledMessages,
    int? DeliveriesBeforeScheduledTime,
    List<RecordedDelivery<OrderPlaced>> Deliveries,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
