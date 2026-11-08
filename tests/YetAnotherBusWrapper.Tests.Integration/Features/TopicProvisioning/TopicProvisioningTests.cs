using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using YetAnotherBusWrapper.Naming;
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
using YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning;

[Collection(EmulatorTestCollection2.CollectionName)]
public class TopicProvisioningTests
{
    private const string Feature = "topic-provisioning";
    private const string TopicLogicalEntityName = "orders";
    private const string BillingSubscriptionLogicalName = "billing";
    private const string ShippingSubscriptionLogicalName = "shipping";
    private const string InstanceName = "instance-01";
    private const int ExistingSubscriptionMaxDeliveryCount = 3;

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public TopicProvisioningTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task TopicProvisioning_ShouldCreateAndDeleteTopicsAndSubscriptionsFromOptions(
        ITopicProvisioningTestCase testCase
    )
    {
        TopicProvisioningTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<TopicProvisioningTestResult> RunAsync(
        ITopicProvisioningTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string topicName = scope.GetTopicName(TopicLogicalEntityName);
        await PrepareExistingEntitiesAsync(testCase, scope, topicName, cancellationToken);
        CreateOptionsRecorder createOptionsRecorder = new(testCase);

        bool started = false;
        bool stopped = false;
        List<TopicStateDto> topicsAfterStart = [];
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, topicName, createOptionsRecorder);
            await host.StartAsync(cancellationToken);
            started = true;
            topicsAfterStart = await GetTopicsAsync(scope, cancellationToken);
            await host.StopAsync(cancellationToken);
            stopped = true;
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        List<TopicStateDto> topicsAfterStop = await GetTopicsAsync(scope, cancellationToken);

        return new TopicProvisioningTestResult(
            testCase.TestCaseId,
            started,
            stopped,
            createOptionsRecorder.TopicCreateOptions,
            createOptionsRecorder.SubscriptionCreateOptions,
            topicsAfterStart,
            topicsAfterStop,
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static async Task PrepareExistingEntitiesAsync(
        ITopicProvisioningTestCase testCase,
        TestCaseScope scope,
        string topicName,
        CancellationToken cancellationToken
    )
    {
        if (!testCase.EntitiesExistBeforeStart)
        {
            return;
        }

        await scope.Inspector.CreateTopicAsync(topicName, cancellationToken);
        foreach (string subscriptionName in (string[])[BillingSubscriptionLogicalName, ShippingSubscriptionLogicalName])
        {
            await scope.Inspector.CreateSubscriptionAsync(
                new CreateSubscriptionOptions(topicName, subscriptionName)
                {
                    MaxDeliveryCount = ExistingSubscriptionMaxDeliveryCount
                },
                cancellationToken
            );
        }
    }

    private static async Task<List<TopicStateDto>> GetTopicsAsync(
        TestCaseScope scope,
        CancellationToken cancellationToken
    )
    {
        List<TopicStateDto> topics = [];
        foreach (string topicName in await scope.GetTopicNamesAsync(cancellationToken))
        {
            TopicPropertiesDto? topic = await scope.Inspector.GetTopicAsync(topicName, cancellationToken);
            if (topic is null)
            {
                continue;
            }

            topics.Add(
                new TopicStateDto(topic, await scope.Inspector.GetSubscriptionsAsync(topicName, cancellationToken))
            );
        }

        return topics;
    }

    private static IHost BuildHost(
        ITopicProvisioningTestCase testCase,
        TestCaseScope scope,
        string topicName,
        CreateOptionsRecorder createOptionsRecorder
    )
    {
        IInstanceNameProvider instanceNameProvider = Substitute.For<IInstanceNameProvider>();
        instanceNameProvider.GetInstanceName().Returns(InstanceName);

        return TestHostBuilder.Build(
            scope.Logs,
            services =>
            {
                services.AddKeyedSingleton(BillingConsumer.RecorderKey, new DeliveryRecorder<OrderPlaced>());
                services.AddKeyedSingleton(ShippingConsumer.RecorderKey, new DeliveryRecorder<OrderPlaced>());
                services.AddYetAnotherBusWrapper(
                    builder =>
                    {
                        builder.UseInstanceNameProvider(instanceNameProvider);
                        builder.AddTopic<OrderPlaced>(TopicLogicalEntityName, createOptionsRecorder.ConfigureTopic)
                            .Configure(
                                options =>
                                {
                                    options.ConnectionString = scope.ConnectionString;
                                    options.AdministrationConnectionString = scope.AdministrationConnectionString;
                                    options.TopicName = topicName;
                                    testCase.ConfigureTopic(options);
                                }
                            );
                        builder.AddTopicConsumer<BillingConsumer, OrderPlaced>(
                                BillingSubscriptionLogicalName,
                                createOptionsRecorder.ConfigureSubscription
                            )
                            .Configure(
                                options => ConfigureSubscription(testCase, BillingSubscriptionLogicalName, options)
                            );
                        builder.AddTopicConsumer<ShippingConsumer, OrderPlaced>(
                                ShippingSubscriptionLogicalName,
                                createOptionsRecorder.ConfigureSubscription
                            )
                            .Configure(
                                options => ConfigureSubscription(testCase, ShippingSubscriptionLogicalName, options)
                            );
                    }
                );
            }
        );
    }

    private static void ConfigureSubscription(
        ITopicProvisioningTestCase testCase,
        string subscriptionLogicalName,
        SubscriptionOptions options
    )
    {
        options.SubscriptionName = subscriptionLogicalName;
        testCase.ConfigureSubscription(subscriptionLogicalName, options);
    }

    private sealed class CreateOptionsRecorder
    {
        private readonly ITopicProvisioningTestCase _testCase;

        public CreateOptionsRecorder(ITopicProvisioningTestCase testCase)
        {
            _testCase = testCase;
        }

        public CreateTopicOptionsDto? TopicCreateOptions { get; private set; }

        public List<CreateSubscriptionOptionsDto> SubscriptionCreateOptions { get; } = [];

        public void ConfigureTopic(CreateTopicOptions options)
        {
            _testCase.ConfigureTopicCreateOptions(options);
            TopicCreateOptions = CreateTopicOptionsDto.From(options);
        }

        public void ConfigureSubscription(CreateSubscriptionOptions options)
        {
            _testCase.ConfigureSubscriptionCreateOptions(options);
            SubscriptionCreateOptions.Add(CreateSubscriptionOptionsDto.From(options));
        }
    }
}

internal record TopicProvisioningTestResult(
    int TestCaseId,
    bool Started,
    bool Stopped,
    CreateTopicOptionsDto? TopicCreateOptions,
    List<CreateSubscriptionOptionsDto> SubscriptionCreateOptions,
    List<TopicStateDto> TopicsAfterStart,
    List<TopicStateDto> TopicsAfterStop,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
