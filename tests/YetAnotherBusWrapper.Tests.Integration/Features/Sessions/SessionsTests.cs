using Azure.Messaging.ServiceBus;
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
using YetAnotherBusWrapper.Tests.Integration.Features.Sessions.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.Sessions.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sessions;

[Collection(EmulatorTestCollection4.CollectionName)]
public class SessionsTests
{
    private const string Feature = "sessions";
    private const string LogicalEntityName = "orders";
    private const string SubscriptionName = "billing";

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(60);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public SessionsTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
        _verifySettings.ScrubMember<MessageContext>(context => context.EnqueuedTime);
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task Sessions_ShouldHandleMessagesInSendOrderWithinEachSession(ISessionsTestCase testCase)
    {
        SessionsTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<SessionsTestResult> RunAsync(ISessionsTestCase testCase, CancellationToken cancellationToken)
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string entityName = await PrepareSessionEntityAsync(testCase, scope, cancellationToken);
        DeliveryRecorder<OrderPlaced> recorder = new();
        ConcurrencyTracker tracker = new();
        List<SendResult> sendResults = [];

        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, entityName, recorder, tracker);
            await host.StartAsync(cancellationToken);
            sendResults.AddRange(await SendAsync(host, testCase.Messages, cancellationToken));
            await recorder.WaitForDeliveriesAsync(testCase.Messages.Count, DeliveryTimeout, cancellationToken);
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        return new SessionsTestResult(
            testCase.TestCaseId,
            testCase.EntityKind,
            testCase.MaxConcurrentSessions,
            testCase.Messages,
            sendResults,
            GroupBySession(recorder.GetDeliveries()),
            tracker.MaxConcurrentDeliveries,
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static async Task<string> PrepareSessionEntityAsync(
        ISessionsTestCase testCase,
        TestCaseScope scope,
        CancellationToken cancellationToken
    )
    {
        if (!testCase.CreateOnStartup)
        {
            return await CreateSessionEntityAsync(testCase, scope, cancellationToken);
        }

        if (testCase.EntityKind == ConsumerEntityKind.Queue)
        {
            return scope.GetQueueName(LogicalEntityName);
        }

        return await scope.CreateTopicAsync(LogicalEntityName, cancellationToken);
    }

    private static async Task<string> CreateSessionEntityAsync(
        ISessionsTestCase testCase,
        TestCaseScope scope,
        CancellationToken cancellationToken
    )
    {
        if (testCase.EntityKind == ConsumerEntityKind.Queue)
        {
            string queueName = scope.GetQueueName(LogicalEntityName);
            await scope.Inspector.CreateQueueAsync(
                new CreateQueueOptions(queueName) { RequiresSession = true },
                cancellationToken
            );

            return queueName;
        }

        string topicName = await scope.CreateTopicAsync(LogicalEntityName, cancellationToken);
        await scope.Inspector.CreateSubscriptionAsync(
            new CreateSubscriptionOptions(topicName, SubscriptionName) { RequiresSession = true },
            cancellationToken
        );

        return topicName;
    }

    private static IHost BuildHost(
        ISessionsTestCase testCase,
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
        ISessionsTestCase testCase,
        TestCaseScope scope,
        string entityName
    )
    {
        if (testCase.EntityKind == ConsumerEntityKind.Queue)
        {
            builder.AddQueue<OrderPlaced>(LogicalEntityName, configureMessage: SetSessionId)
                .Configure(
                    options =>
                    {
                        options.ConnectionString = scope.ConnectionString;
                        options.AdministrationConnectionString = scope.AdministrationConnectionString;
                        options.QueueName = entityName;
                        options.CreateOnStartup = testCase.CreateOnStartup;
                        options.FailStartupOnProvisioningError = true;
                    }
                );
            builder.AddQueueConsumer<HoldingConsumer, OrderPlaced>()
                .Configure(
                    options =>
                    {
                        options.SessionsEnabled = true;
                        options.MaxConcurrentSessions = testCase.MaxConcurrentSessions;
                        options.SessionIdleTimeout = testCase.SessionIdleTimeout;
                    }
                );

            return;
        }

        builder.AddTopic<OrderPlaced>(LogicalEntityName, configureMessage: SetSessionId)
            .Configure(
                options =>
                {
                    options.ConnectionString = scope.ConnectionString;
                    options.AdministrationConnectionString = scope.AdministrationConnectionString;
                    options.TopicName = entityName;
                }
            );
        builder.AddTopicConsumer<HoldingConsumer, OrderPlaced>(SubscriptionName)
            .Configure(
                options =>
                {
                    options.SubscriptionName = SubscriptionName;
                    options.CreateOnStartup = testCase.CreateOnStartup;
                    options.FailStartupOnProvisioningError = true;
                    options.SessionsEnabled = true;
                    options.MaxConcurrentSessions = testCase.MaxConcurrentSessions;
                    options.SessionIdleTimeout = testCase.SessionIdleTimeout;
                }
            );
    }

    private static void SetSessionId(ServiceBusMessage serviceBusMessage, OrderPlaced message)
    {
        serviceBusMessage.SessionId = message.OrderId[..message.OrderId.LastIndexOf('-')];
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

    private static List<SessionDeliveriesDto> GroupBySession(List<RecordedDelivery<OrderPlaced>> deliveries)
    {
        return deliveries.GroupBy(delivery => delivery.Context.SessionId)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(
                group => new SessionDeliveriesDto(
                    group.Key,
                    group.Select(delivery => new SessionDeliveryDto(delivery.Message, delivery.Context)).ToList()
                )
            )
            .ToList();
    }
}

internal record SessionsTestResult(
    int TestCaseId,
    ConsumerEntityKind EntityKind,
    int MaxConcurrentSessions,
    IReadOnlyList<OrderPlaced> SentMessages,
    List<SendResult> SendResults,
    List<SessionDeliveriesDto> SessionDeliveries,
    int MaxConcurrentDeliveries,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
