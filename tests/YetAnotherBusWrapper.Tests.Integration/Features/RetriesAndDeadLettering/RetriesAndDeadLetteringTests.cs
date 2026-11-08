using System.Text;
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
using YetAnotherBusWrapper.Tests.Integration.Features.RetriesAndDeadLettering.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.RetriesAndDeadLettering.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RetriesAndDeadLettering;

[Collection(EmulatorTestCollection4.CollectionName)]
public class RetriesAndDeadLetteringTests
{
    private const string Feature = "retries";
    private const string LogicalEntityName = "orders";
    private const string JsonContentType = "application/json";
    private const string MessageSenderLogger = "MessageSender";

    private static readonly TimeSpan SettlementTimeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(200);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public RetriesAndDeadLetteringTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task RetriesAndDeadLettering_ShouldRedeliverFailedMessagesUntilMaxDeliveryCount(
        IRetriesAndDeadLetteringTestCase testCase
    )
    {
        RetriesAndDeadLetteringTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<RetriesAndDeadLetteringTestResult> RunAsync(
        IRetriesAndDeadLetteringTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string queueName = scope.GetQueueName(LogicalEntityName);
        DeliveryRecorder<OrderPlaced> recorder = new();

        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, queueName, recorder);
            await host.StartAsync(cancellationToken);
            await SendMessagesAsync(host, testCase.Messages, cancellationToken);
            await SendRawMessagesAsync(scope, queueName, testCase.RawMessages, cancellationToken);
            await recorder.WaitForDeliveriesAsync(testCase.DeliveriesToWaitFor, SettlementTimeout, cancellationToken);
            await WaitForDeadLetteredMessagesAsync(scope, queueName, testCase, cancellationToken);
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        List<PeekedMessageDto> remainingMessages = await scope.Inspector.PeekQueueAsync(queueName, cancellationToken);
        List<DeadLetteredMessageDto> deadLetteredMessages = await scope.Inspector.PeekQueueDeadLetterAsync(
            queueName,
            cancellationToken
        );

        return new RetriesAndDeadLetteringTestResult(
            testCase.TestCaseId,
            testCase.MaxDeliveryCount,
            testCase.FailurePlan,
            testCase.Messages,
            testCase.RawMessages,
            recorder.GetDeliveries(),
            remainingMessages,
            deadLetteredMessages,
            scope.Logs.GetLogsWithScopes().Where(log => log.Logger != MessageSenderLogger).ToList(),
            thrownException
        );
    }

    private static IHost BuildHost(
        IRetriesAndDeadLetteringTestCase testCase,
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
                        builder.AddQueue<OrderPlaced>(LogicalEntityName)
                            .Configure(
                                options =>
                                {
                                    options.ConnectionString = scope.ConnectionString;
                                    options.AdministrationConnectionString = scope.AdministrationConnectionString;
                                    options.QueueName = queueName;
                                    options.CreateOnStartup = true;
                                    options.FailStartupOnProvisioningError = true;
                                    options.MaxDeliveryCount = testCase.MaxDeliveryCount;
                                }
                            );
                        builder.AddQueueConsumer<FailingConsumer, OrderPlaced>();
                    }
                );
            }
        );
    }

    private static async Task SendMessagesAsync(
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

    private static async Task SendRawMessagesAsync(
        TestCaseScope scope,
        string queueName,
        IReadOnlyList<RawMessageDto> rawMessages,
        CancellationToken cancellationToken
    )
    {
        foreach (RawMessageDto rawMessage in rawMessages)
        {
            ServiceBusMessage message = new(Encoding.UTF8.GetBytes(rawMessage.Body))
            {
                MessageId = rawMessage.MessageId,
                ContentType = JsonContentType
            };
            await scope.Inspector.SendToQueueAsync(queueName, message, cancellationToken);
        }
    }

    private static async Task WaitForDeadLetteredMessagesAsync(
        TestCaseScope scope,
        string queueName,
        IRetriesAndDeadLetteringTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        using CancellationTokenSource timeoutSource = new(SettlementTimeout);
        while (!timeoutSource.IsCancellationRequested)
        {
            List<DeadLetteredMessageDto> deadLetteredMessages = await scope.Inspector.PeekQueueDeadLetterAsync(
                queueName,
                cancellationToken
            );
            if (deadLetteredMessages.Count >= testCase.DeadLetteredMessagesToWaitFor)
            {
                return;
            }

            await Task.Delay(PollInterval, cancellationToken);
        }
    }
}

internal record RetriesAndDeadLetteringTestResult(
    int TestCaseId,
    int MaxDeliveryCount,
    ConsumerFailurePlan FailurePlan,
    IReadOnlyList<OrderPlaced> SentMessages,
    IReadOnlyList<RawMessageDto> SentRawMessages,
    List<RecordedDelivery<OrderPlaced>> Deliveries,
    List<PeekedMessageDto> RemainingMessages,
    List<DeadLetteredMessageDto> DeadLetteredMessages,
    List<CapturedScopedLogDto> Logs,
    ThrownException? ThrownException
);
