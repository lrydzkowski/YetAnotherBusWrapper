using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
using YetAnotherBusWrapper.Tests.Integration.Features.ClientSharing.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.ClientSharing.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ClientSharing;

[Collection(EmulatorTestCollection3.CollectionName)]
public class ClientSharingTests
{
    private const string Feature = "client-sharing";
    private const string QueueLogicalEntityName = "reports";
    private const string TopicLogicalEntityName = "orders";

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(30);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public ClientSharingTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task ClientSharing_ShouldShareOneClientPerConnectionString(IClientSharingTestCase testCase)
    {
        ClientSharingTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<ClientSharingTestResult> RunAsync(
        IClientSharingTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        DeliveryRecorder<ReportGenerated> queueRecorder = new();
        DeliveryRecorder<OrderPlaced> subscriptionRecorder = new();

        bool started = false;
        RetryOptionsDto? retryOptions = null;
        SendOutcomeDto? queueSend = null;
        SendOutcomeDto? topicSend = null;
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, queueRecorder, subscriptionRecorder);
            retryOptions = GetRetryOptions(host);
            await host.StartAsync(cancellationToken);
            started = true;
            queueSend = await SendOutcomeDto.CaptureAsync(
                () => host.Services.GetRequiredService<IMessageSender<ReportGenerated>>()
                    .SendAsync(new ReportGenerated(1, "report-1"), cancellationToken: cancellationToken)
            );
            topicSend = await SendOutcomeDto.CaptureAsync(
                () => host.Services.GetRequiredService<IMessageSender<OrderPlaced>>()
                    .SendAsync(new OrderPlaced("order-001", 2, OrderStatus.New), cancellationToken: cancellationToken)
            );
            await WaitForDeliveriesAsync(queueSend, queueRecorder, 1, cancellationToken);
            await WaitForDeliveriesAsync(
                topicSend,
                subscriptionRecorder,
                testCase.SubscriptionCount,
                cancellationToken
            );
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        return new ClientSharingTestResult(
            testCase.TestCaseId,
            retryOptions,
            started,
            queueSend,
            topicSend,
            queueRecorder.GetDeliveries().Count,
            subscriptionRecorder.GetDeliveries().Count,
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static async Task WaitForDeliveriesAsync<TMessage>(
        SendOutcomeDto send,
        DeliveryRecorder<TMessage> recorder,
        int expectedCount,
        CancellationToken cancellationToken
    )
    {
        if (send.Result?.Status != SendStatus.Sent)
        {
            return;
        }

        await recorder.WaitForDeliveriesAsync(expectedCount, DeliveryTimeout, cancellationToken);
    }

    private static RetryOptionsDto GetRetryOptions(IHost host)
    {
        QueueOptions queueOptions = host.Services.GetRequiredService<IOptionsMonitor<QueueOptions>>()
            .Get(QueueLogicalEntityName);
        TopicOptions topicOptions = host.Services.GetRequiredService<IOptionsMonitor<TopicOptions>>()
            .Get(TopicLogicalEntityName);

        return new RetryOptionsDto(queueOptions.ClientRetry, topicOptions.ClientRetry);
    }

    private static IHost BuildHost(
        IClientSharingTestCase testCase,
        TestCaseScope scope,
        DeliveryRecorder<ReportGenerated> queueRecorder,
        DeliveryRecorder<OrderPlaced> subscriptionRecorder
    )
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services =>
            {
                services.AddSingleton(queueRecorder);
                services.AddSingleton(subscriptionRecorder);
                services.AddYetAnotherBusWrapper(
                    builder =>
                    {
                        builder.AddQueue<ReportGenerated>(QueueLogicalEntityName)
                            .Configure(options => ConfigureQueue(testCase, scope, options));
                        builder.AddQueueConsumer<RecordingConsumer<ReportGenerated>, ReportGenerated>();
                        builder.AddTopic<OrderPlaced>(TopicLogicalEntityName)
                            .Configure(options => ConfigureTopic(testCase, scope, options));
                        AddSubscriptions(builder, testCase.SubscriptionCount);
                        testCase.Configure(builder);
                    }
                );
            }
        );
    }

    private static void AddSubscriptions(YetAnotherBusWrapperBuilder builder, int subscriptionCount)
    {
        for (int number = 1; number <= subscriptionCount; number++)
        {
            string subscriptionLogicalName = $"subscription-{number:D2}";
            builder.AddTopicConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>(subscriptionLogicalName)
                .Configure(
                    options =>
                    {
                        options.SubscriptionName = subscriptionLogicalName;
                        options.CreateOnStartup = true;
                    }
                );
        }
    }

    private static void ConfigureQueue(IClientSharingTestCase testCase, TestCaseScope scope, QueueOptions options)
    {
        options.ConnectionString = scope.ConnectionString;
        options.AdministrationConnectionString = scope.AdministrationConnectionString;
        options.QueueName = scope.GetQueueName(QueueLogicalEntityName);
        options.CreateOnStartup = true;
        testCase.ConfigureQueue(options);
    }

    private static void ConfigureTopic(IClientSharingTestCase testCase, TestCaseScope scope, TopicOptions options)
    {
        options.ConnectionString = scope.ConnectionString;
        options.AdministrationConnectionString = scope.AdministrationConnectionString;
        options.TopicName = scope.GetTopicName(TopicLogicalEntityName);
        options.CreateOnStartup = true;
        testCase.ConfigureTopic(options);
    }
}

internal record RetryOptionsDto(ClientRetryOptions Queue, ClientRetryOptions Topic);

internal record ClientSharingTestResult(
    int TestCaseId,
    RetryOptionsDto? RetryOptions,
    bool Started,
    SendOutcomeDto? QueueSend,
    SendOutcomeDto? TopicSend,
    int QueueDeliveryCount,
    int SubscriptionDeliveryCount,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
