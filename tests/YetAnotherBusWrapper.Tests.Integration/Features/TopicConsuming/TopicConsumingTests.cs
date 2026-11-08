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
using YetAnotherBusWrapper.Tests.Integration.Features.TopicConsuming.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.TopicConsuming.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicConsuming;

[Collection(EmulatorTestCollection4.CollectionName)]
public class TopicConsumingTests
{
    private const string Feature = "topic-consuming";
    private const string TopicLogicalEntityName = "orders";
    private const string BillingSubscriptionName = "billing";
    private const string ShippingSubscriptionName = "shipping";

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(30);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public TopicConsumingTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
        _verifySettings.ScrubMember<MessageContext>(context => context.EnqueuedTime);
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task TopicConsuming_ShouldDeliverCopyToEverySubscription(ITopicConsumingTestCase testCase)
    {
        TopicConsumingTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<TopicConsumingTestResult> RunAsync(
        ITopicConsumingTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string topicName = await CreateTopicWithSubscriptionsAsync(scope, cancellationToken);
        DeliveryRecorder<OrderPlaced> billingRecorder = new();
        DeliveryRecorder<OrderPlaced> shippingRecorder = new();
        List<SendResult> sendResults = [];

        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, topicName, billingRecorder, shippingRecorder);
            await host.StartAsync(cancellationToken);
            sendResults.AddRange(await SendAsync(host, testCase.Messages, cancellationToken));
            await billingRecorder.WaitForDeliveriesAsync(testCase.Messages.Count, DeliveryTimeout, cancellationToken);
            await shippingRecorder.WaitForDeliveriesAsync(testCase.Messages.Count, DeliveryTimeout, cancellationToken);
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        return new TopicConsumingTestResult(
            testCase.TestCaseId,
            testCase.Messages,
            sendResults,
            billingRecorder.GetDeliveries(),
            shippingRecorder.GetDeliveries(),
            await scope.Inspector.PeekSubscriptionAsync(topicName, BillingSubscriptionName, cancellationToken),
            await scope.Inspector.PeekSubscriptionAsync(topicName, ShippingSubscriptionName, cancellationToken),
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static async Task<string> CreateTopicWithSubscriptionsAsync(
        TestCaseScope scope,
        CancellationToken cancellationToken
    )
    {
        string topicName = await scope.CreateTopicAsync(TopicLogicalEntityName, cancellationToken);
        await scope.Inspector.CreateSubscriptionAsync(topicName, BillingSubscriptionName, cancellationToken);
        await scope.Inspector.CreateSubscriptionAsync(topicName, ShippingSubscriptionName, cancellationToken);

        return topicName;
    }

    private static IHost BuildHost(
        ITopicConsumingTestCase testCase,
        TestCaseScope scope,
        string topicName,
        DeliveryRecorder<OrderPlaced> billingRecorder,
        DeliveryRecorder<OrderPlaced> shippingRecorder
    )
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services =>
            {
                services.AddKeyedSingleton(BillingConsumer.RecorderKey, billingRecorder);
                services.AddKeyedSingleton(ShippingConsumer.RecorderKey, shippingRecorder);
                services.AddYetAnotherBusWrapper(
                    builder =>
                    {
                        builder.AddTopic<OrderPlaced>(
                                TopicLogicalEntityName,
                                configureMessage: testCase.ConfigureMessage
                            )
                            .Configure(
                                options =>
                                {
                                    options.ConnectionString = scope.ConnectionString;
                                    options.TopicName = topicName;
                                }
                            );
                        builder.AddTopicConsumer<BillingConsumer, OrderPlaced>(BillingSubscriptionName)
                            .Configure(options => options.SubscriptionName = BillingSubscriptionName);
                        builder.AddTopicConsumer<ShippingConsumer, OrderPlaced>(ShippingSubscriptionName)
                            .Configure(options => options.SubscriptionName = ShippingSubscriptionName);
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
}

internal record TopicConsumingTestResult(
    int TestCaseId,
    IReadOnlyList<OrderPlaced> SentMessages,
    List<SendResult> SendResults,
    List<RecordedDelivery<OrderPlaced>> BillingDeliveries,
    List<RecordedDelivery<OrderPlaced>> ShippingDeliveries,
    List<PeekedMessageDto> RemainingBillingMessages,
    List<PeekedMessageDto> RemainingShippingMessages,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
