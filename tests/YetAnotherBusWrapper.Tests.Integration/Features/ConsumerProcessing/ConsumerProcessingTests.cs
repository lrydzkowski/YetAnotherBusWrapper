using Azure.Messaging.ServiceBus.Administration;
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
using YetAnotherBusWrapper.Tests.Integration.Features.ConsumerProcessing.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.ConsumerProcessing.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ConsumerProcessing;

[Collection(EmulatorTestCollection1.CollectionName)]
public class ConsumerProcessingTests
{
    private const string Feature = "consumer-processing";
    private const string LogicalEntityName = "orders";
    private const string SubscriptionName = "billing";

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(60);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public ConsumerProcessingTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task ConsumerProcessing_ShouldApplyProcessingOptionsToConsumer(IConsumerProcessingTestCase testCase)
    {
        ConsumerProcessingTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<ConsumerProcessingTestResult> RunAsync(
        IConsumerProcessingTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string entityName = await CreateEntityAsync(testCase, scope, cancellationToken);
        DeliveryRecorder<OrderPlaced> recorder = new();
        ConcurrencyTracker tracker = new();
        List<SendResult> sendResults = [];

        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, entityName, recorder, tracker);
            await host.StartAsync(cancellationToken);
            sendResults.AddRange(await SendAsync(host, testCase.Messages, cancellationToken));
            await recorder.WaitForDeliveriesAsync(testCase.DeliveriesToWaitFor, DeliveryTimeout, cancellationToken);
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        return new ConsumerProcessingTestResult(
            testCase.TestCaseId,
            testCase.EntityKind,
            testCase.LockDuration,
            testCase.MaxConcurrentCalls,
            testCase.MaxAutoLockRenewalDuration,
            testCase.Messages,
            sendResults,
            GetHandledDeliveries(recorder),
            tracker.MaxConcurrentDeliveries,
            await PeekRemainingMessagesAsync(testCase, scope, entityName, cancellationToken),
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static async Task<string> CreateEntityAsync(
        IConsumerProcessingTestCase testCase,
        TestCaseScope scope,
        CancellationToken cancellationToken
    )
    {
        if (testCase.EntityKind == ConsumerEntityKind.Queue)
        {
            string queueName = scope.GetQueueName(LogicalEntityName);
            CreateQueueOptions queueOptions = new(queueName);
            if (testCase.LockDuration is { } queueLockDuration)
            {
                queueOptions.LockDuration = queueLockDuration;
            }

            await scope.Inspector.CreateQueueAsync(queueOptions, cancellationToken);

            return queueName;
        }

        string topicName = await scope.CreateTopicAsync(LogicalEntityName, cancellationToken);
        CreateSubscriptionOptions subscriptionOptions = new(topicName, SubscriptionName);
        if (testCase.LockDuration is { } subscriptionLockDuration)
        {
            subscriptionOptions.LockDuration = subscriptionLockDuration;
        }

        await scope.Inspector.CreateSubscriptionAsync(subscriptionOptions, cancellationToken);

        return topicName;
    }

    private static IHost BuildHost(
        IConsumerProcessingTestCase testCase,
        TestCaseScope scope,
        string entityName,
        DeliveryRecorder<OrderPlaced> recorder,
        ConcurrencyTracker tracker
    )
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services =>
            {
                services.AddSingleton(recorder);
                services.AddSingleton(tracker);
                services.AddSingleton(testCase.HoldPlan);
                services.AddYetAnotherBusWrapper(builder => AddEntityAndConsumer(builder, testCase, scope, entityName));
            }
        );
    }

    private static void AddEntityAndConsumer(
        YetAnotherBusWrapperBuilder builder,
        IConsumerProcessingTestCase testCase,
        TestCaseScope scope,
        string entityName
    )
    {
        if (testCase.EntityKind == ConsumerEntityKind.Queue)
        {
            builder.AddQueue<OrderPlaced>(LogicalEntityName)
                .Configure(
                    options =>
                    {
                        options.ConnectionString = scope.ConnectionString;
                        options.QueueName = entityName;
                    }
                );
            builder.AddQueueConsumer<HoldingConsumer, OrderPlaced>()
                .Configure(
                    options =>
                    {
                        options.MaxConcurrentCalls = testCase.MaxConcurrentCalls ?? options.MaxConcurrentCalls;
                        options.MaxAutoLockRenewalDuration =
                            testCase.MaxAutoLockRenewalDuration ?? options.MaxAutoLockRenewalDuration;
                    }
                );

            return;
        }

        builder.AddTopic<OrderPlaced>(LogicalEntityName)
            .Configure(
                options =>
                {
                    options.ConnectionString = scope.ConnectionString;
                    options.TopicName = entityName;
                }
            );
        builder.AddTopicConsumer<HoldingConsumer, OrderPlaced>(SubscriptionName)
            .Configure(
                options =>
                {
                    options.SubscriptionName = SubscriptionName;
                    options.MaxConcurrentCalls = testCase.MaxConcurrentCalls ?? options.MaxConcurrentCalls;
                    options.MaxAutoLockRenewalDuration =
                        testCase.MaxAutoLockRenewalDuration ?? options.MaxAutoLockRenewalDuration;
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

    private static List<HandledDeliveryDto> GetHandledDeliveries(DeliveryRecorder<OrderPlaced> recorder)
    {
        return recorder.GetDeliveries()
            .Select(delivery => new HandledDeliveryDto(delivery.Message, delivery.Context.DeliveryCount))
            .OrderBy(delivery => delivery.Message.OrderId, StringComparer.Ordinal)
            .ThenBy(delivery => delivery.DeliveryCount)
            .ToList();
    }

    private static async Task<List<PeekedMessageDto>> PeekRemainingMessagesAsync(
        IConsumerProcessingTestCase testCase,
        TestCaseScope scope,
        string entityName,
        CancellationToken cancellationToken
    )
    {
        if (testCase.EntityKind == ConsumerEntityKind.Queue)
        {
            return await scope.Inspector.PeekQueueAsync(entityName, cancellationToken);
        }

        return await scope.Inspector.PeekSubscriptionAsync(entityName, SubscriptionName, cancellationToken);
    }
}

internal record ConsumerProcessingTestResult(
    int TestCaseId,
    ConsumerEntityKind EntityKind,
    TimeSpan? LockDuration,
    int? MaxConcurrentCalls,
    TimeSpan? MaxAutoLockRenewalDuration,
    IReadOnlyList<OrderPlaced> SentMessages,
    List<SendResult> SendResults,
    List<HandledDeliveryDto> HandledDeliveries,
    int MaxConcurrentDeliveries,
    List<PeekedMessageDto> RemainingMessages,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
