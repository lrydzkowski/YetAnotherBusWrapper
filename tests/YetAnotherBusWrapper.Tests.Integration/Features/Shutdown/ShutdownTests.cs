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
using YetAnotherBusWrapper.Tests.Integration.Features.Shutdown.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.Shutdown.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Shutdown;

[Collection(EmulatorTestCollection2.CollectionName)]
public class ShutdownTests
{
    private const string Feature = "shutdown";
    private const string TopicLogicalEntityName = "orders";
    private const string BillingSubscriptionName = "billing";
    private const string ShippingSubscriptionName = "shipping";

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(30);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public ShutdownTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task Shutdown_ShouldStopEveryConsumerAndDeleteEntities(IShutdownTestCase testCase)
    {
        ShutdownTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<ShutdownTestResult> RunAsync(IShutdownTestCase testCase, CancellationToken cancellationToken)
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string topicName = scope.GetTopicName(TopicLogicalEntityName);
        BusyConsumerSignal billingSignal = new(testCase.ConsumerBusyDuration, testCase.ConsumerObservesCancellation);
        DeliveryRecorder<OrderPlaced> shippingRecorder = new();

        bool stopped = false;
        List<SubscriptionPropertiesDto> subscriptionsAfterStart = [];
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(scope, topicName, billingSignal, shippingRecorder);
            await host.StartAsync(cancellationToken);
            subscriptionsAfterStart = await scope.Inspector.GetSubscriptionsAsync(topicName, cancellationToken);
            await SendAsync(host, cancellationToken);
            await billingSignal.WaitUntilStartedAsync(DeliveryTimeout, cancellationToken);
            await shippingRecorder.WaitForDeliveriesAsync(1, DeliveryTimeout, cancellationToken);
            await StopAsync(host, testCase.StopTimeout, cancellationToken);
            stopped = true;
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        return new ShutdownTestResult(
            testCase.TestCaseId,
            stopped,
            subscriptionsAfterStart,
            await scope.Inspector.GetSubscriptionsAsync(topicName, cancellationToken),
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static IHost BuildHost(
        TestCaseScope scope,
        string topicName,
        BusyConsumerSignal billingSignal,
        DeliveryRecorder<OrderPlaced> shippingRecorder
    )
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services =>
            {
                services.AddSingleton(billingSignal);
                services.AddKeyedSingleton(ShippingConsumer.RecorderKey, shippingRecorder);
                services.AddYetAnotherBusWrapper(
                    builder =>
                    {
                        builder.AddTopic<OrderPlaced>(TopicLogicalEntityName)
                            .Configure(
                                options =>
                                {
                                    options.ConnectionString = scope.ConnectionString;
                                    options.AdministrationConnectionString = scope.AdministrationConnectionString;
                                    options.TopicName = topicName;
                                    options.CreateOnStartup = true;
                                }
                            );
                        builder.AddTopicConsumer<BusyConsumer, OrderPlaced>(BillingSubscriptionName)
                            .Configure(options => ConfigureSubscription(BillingSubscriptionName, options));
                        builder.AddTopicConsumer<ShippingConsumer, OrderPlaced>(ShippingSubscriptionName)
                            .Configure(options => ConfigureSubscription(ShippingSubscriptionName, options));
                    }
                );
            }
        );
    }

    private static void ConfigureSubscription(string subscriptionName, SubscriptionOptions options)
    {
        options.SubscriptionName = subscriptionName;
        options.CreateOnStartup = true;
        options.DeleteOnShutdown = true;
    }

    private static async Task SendAsync(IHost host, CancellationToken cancellationToken)
    {
        IMessageSender<OrderPlaced> sender = host.Services.GetRequiredService<IMessageSender<OrderPlaced>>();
        await sender.SendAsync(new OrderPlaced("order-1", 1, OrderStatus.New), cancellationToken: cancellationToken);
    }

    private static async Task StopAsync(IHost host, TimeSpan stopTimeout, CancellationToken cancellationToken)
    {
        using CancellationTokenSource stopSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        stopSource.CancelAfter(stopTimeout);
        await host.StopAsync(stopSource.Token);
    }
}

internal record ShutdownTestResult(
    int TestCaseId,
    bool Stopped,
    List<SubscriptionPropertiesDto> SubscriptionsAfterStart,
    List<SubscriptionPropertiesDto> SubscriptionsAfterStop,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
