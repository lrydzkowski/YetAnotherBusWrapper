using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;
using YetAnotherBusWrapper.Tests.Integration.Common.Host;
using YetAnotherBusWrapper.Tests.Integration.Common.Logging;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;
using YetAnotherBusWrapper.Tests.Integration.Common.Services;
using YetAnotherBusWrapper.Tests.Integration.Common.TestCollections;
using YetAnotherBusWrapper.Tests.Integration.Features.DisabledEntities.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.DisabledEntities.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.DisabledEntities;

[Collection(EmulatorTestCollection4.CollectionName)]
public class DisabledEntitiesTests
{
    public const string BillingSubscriptionLogicalName = "billing";
    public const string ShippingSubscriptionLogicalName = "shipping";

    private const string Feature = "disabled-entities";
    private const string QueueLogicalEntityName = "reports";
    private const string TopicLogicalEntityName = "orders";

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(30);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public DisabledEntitiesTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
        _verifySettings.ScrubMember<MessageContext>(context => context.EnqueuedTime);
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task DisabledEntities_ShouldNotBeProvisionedConsumedOrSentTo(IDisabledEntitiesTestCase testCase)
    {
        DisabledEntitiesTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<DisabledEntitiesTestResult> RunAsync(
        IDisabledEntitiesTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        DeliveryRecorder<ReportGenerated> queueRecorder = new();
        DeliveryRecorder<OrderPlaced> billingRecorder = new();
        DeliveryRecorder<OrderPlaced> shippingRecorder = new();

        bool started = false;
        SendOutcomeDto? queueSend = null;
        SendOutcomeDto? topicSend = null;
        EntitiesDto? entitiesAfterStart = null;
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, queueRecorder, billingRecorder, shippingRecorder);
            await host.StartAsync(cancellationToken);
            started = true;
            entitiesAfterStart = await GetEntitiesAsync(scope, cancellationToken);
            queueSend = await SendOutcomeDto.CaptureAsync(
                () => host.Services.GetRequiredService<IMessageSender<ReportGenerated>>()
                    .SendAsync(new ReportGenerated(1, "report-1"), cancellationToken: cancellationToken)
            );
            topicSend = await SendOutcomeDto.CaptureAsync(
                () => host.Services.GetRequiredService<IMessageSender<OrderPlaced>>()
                    .SendAsync(new OrderPlaced("order-001", 2, OrderStatus.New), cancellationToken: cancellationToken)
            );
            await queueRecorder.WaitForDeliveriesAsync(
                testCase.ExpectedQueueDeliveries,
                DeliveryTimeout,
                cancellationToken
            );
            await billingRecorder.WaitForDeliveriesAsync(
                testCase.ExpectedBillingDeliveries,
                DeliveryTimeout,
                cancellationToken
            );
            await shippingRecorder.WaitForDeliveriesAsync(
                testCase.ExpectedShippingDeliveries,
                DeliveryTimeout,
                cancellationToken
            );
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        return new DisabledEntitiesTestResult(
            testCase.TestCaseId,
            started,
            entitiesAfterStart,
            queueSend,
            topicSend,
            queueRecorder.GetDeliveries(),
            billingRecorder.GetDeliveries(),
            shippingRecorder.GetDeliveries(),
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static async Task<EntitiesDto> GetEntitiesAsync(TestCaseScope scope, CancellationToken cancellationToken)
    {
        List<string> topics = await scope.GetTopicNamesAsync(cancellationToken);
        List<string> subscriptions = [];
        foreach (string topicName in topics)
        {
            List<SubscriptionPropertiesDto> topicSubscriptions = await scope.Inspector.GetSubscriptionsAsync(
                topicName,
                cancellationToken
            );
            subscriptions.AddRange(
                topicSubscriptions.Select(subscription => $"{subscription.TopicName}/{subscription.SubscriptionName}")
            );
        }

        return new EntitiesDto(await scope.GetQueueNamesAsync(cancellationToken), topics, subscriptions);
    }

    private static IHost BuildHost(
        IDisabledEntitiesTestCase testCase,
        TestCaseScope scope,
        DeliveryRecorder<ReportGenerated> queueRecorder,
        DeliveryRecorder<OrderPlaced> billingRecorder,
        DeliveryRecorder<OrderPlaced> shippingRecorder
    )
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services =>
            {
                services.AddSingleton(queueRecorder);
                services.AddKeyedSingleton(BillingConsumer.RecorderKey, billingRecorder);
                services.AddKeyedSingleton(ShippingConsumer.RecorderKey, shippingRecorder);
                services.AddYetAnotherBusWrapper(
                    builder =>
                    {
                        builder.AddQueue<ReportGenerated>(QueueLogicalEntityName)
                            .Configure(options => ConfigureQueue(testCase, scope, options));
                        builder.AddQueueConsumer<RecordingConsumer<ReportGenerated>, ReportGenerated>();
                        builder.AddTopic<OrderPlaced>(TopicLogicalEntityName)
                            .Configure(options => ConfigureTopic(testCase, scope, options));
                        builder.AddTopicConsumer<BillingConsumer, OrderPlaced>(BillingSubscriptionLogicalName)
                            .Configure(
                                options => ConfigureSubscription(testCase, BillingSubscriptionLogicalName, options)
                            );
                        builder.AddTopicConsumer<ShippingConsumer, OrderPlaced>(ShippingSubscriptionLogicalName)
                            .Configure(
                                options => ConfigureSubscription(testCase, ShippingSubscriptionLogicalName, options)
                            );
                    }
                );
            }
        );
    }

    private static void ConfigureQueue(IDisabledEntitiesTestCase testCase, TestCaseScope scope, QueueOptions options)
    {
        options.ConnectionString = scope.ConnectionString;
        options.AdministrationConnectionString = scope.AdministrationConnectionString;
        options.QueueName = scope.GetQueueName(QueueLogicalEntityName);
        options.CreateOnStartup = true;
        testCase.ConfigureQueue(options);
    }

    private static void ConfigureTopic(IDisabledEntitiesTestCase testCase, TestCaseScope scope, TopicOptions options)
    {
        options.ConnectionString = scope.ConnectionString;
        options.AdministrationConnectionString = scope.AdministrationConnectionString;
        options.TopicName = scope.GetTopicName(TopicLogicalEntityName);
        options.CreateOnStartup = true;
        testCase.ConfigureTopic(options);
    }

    private static void ConfigureSubscription(
        IDisabledEntitiesTestCase testCase,
        string subscriptionLogicalName,
        SubscriptionOptions options
    )
    {
        options.SubscriptionName = subscriptionLogicalName;
        options.CreateOnStartup = true;
        testCase.ConfigureSubscription(subscriptionLogicalName, options);
    }
}

internal record DisabledEntitiesTestResult(
    int TestCaseId,
    bool Started,
    EntitiesDto? EntitiesAfterStart,
    SendOutcomeDto? QueueSend,
    SendOutcomeDto? TopicSend,
    List<RecordedDelivery<ReportGenerated>> QueueDeliveries,
    List<RecordedDelivery<OrderPlaced>> BillingDeliveries,
    List<RecordedDelivery<OrderPlaced>> ShippingDeliveries,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
