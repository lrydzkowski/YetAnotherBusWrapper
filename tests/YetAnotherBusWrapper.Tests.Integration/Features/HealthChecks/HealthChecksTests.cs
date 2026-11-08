using System.Text.RegularExpressions;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
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
using YetAnotherBusWrapper.Tests.Integration.Features.HealthChecks.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.HealthChecks.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.HealthChecks;

[Collection(EmulatorTestCollection1.CollectionName)]
public partial class HealthChecksTests
{
    public const string BillingSubscriptionLogicalName = "billing";
    public const string ShippingSubscriptionLogicalName = "shipping";

    private const string Feature = "health-checks";
    private const string QueueLogicalEntityName = "reports";
    private const string TopicLogicalEntityName = "orders";
    private const string HealthCheckServiceLogger = "DefaultHealthCheckService";

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public HealthChecksTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
        _verifySettings.ScrubLinesWithReplace(
            line =>
            {
                line = TrackingIdRegex().Replace(line, "TrackingId:{Scrubbed}");
                line = TimestampRegex().Replace(line, "Timestamp:{Scrubbed}");

                return line;
            }
        );
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [GeneratedRegex(@"TrackingId:[^,\s]+")]
    private static partial Regex TrackingIdRegex();

    [GeneratedRegex(@"Timestamp:[^\s)]+")]
    private static partial Regex TimestampRegex();

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task HealthChecks_ShouldReportStateOfEachEntity(IHealthChecksTestCase testCase)
    {
        HealthChecksTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<HealthChecksTestResult> RunAsync(
        IHealthChecksTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        await PrepareEntitiesAsync(testCase, scope, cancellationToken);

        HealthStatus? overallStatus = null;
        List<HealthCheckEntryDto> entries = [];
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope);
            HealthReport report = await host.Services.GetRequiredService<HealthCheckService>()
                .CheckHealthAsync(cancellationToken);
            overallStatus = report.Status;
            entries = report.Entries.OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => HealthCheckEntryDto.From(entry.Key, entry.Value))
                .ToList();
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        return new HealthChecksTestResult(
            testCase.TestCaseId,
            overallStatus,
            entries,
            scope.Logs.GetLogs().Where(log => log.Logger != HealthCheckServiceLogger).ToList(),
            thrownException
        );
    }

    private static async Task PrepareEntitiesAsync(
        IHealthChecksTestCase testCase,
        TestCaseScope scope,
        CancellationToken cancellationToken
    )
    {
        if (!testCase.EntitiesExist)
        {
            return;
        }

        await scope.Inspector.CreateQueueAsync(
            new CreateQueueOptions(scope.GetQueueName(QueueLogicalEntityName))
            {
                RequiresSession = testCase.EntitiesRequireSession
            },
            cancellationToken
        );
        string topicName = await scope.CreateTopicAsync(TopicLogicalEntityName, cancellationToken);
        foreach (string subscriptionName in (string[])[BillingSubscriptionLogicalName, ShippingSubscriptionLogicalName])
        {
            await scope.Inspector.CreateSubscriptionAsync(
                new CreateSubscriptionOptions(topicName, subscriptionName)
                {
                    RequiresSession = testCase.EntitiesRequireSession
                },
                cancellationToken
            );
        }
    }

    private static IHost BuildHost(IHealthChecksTestCase testCase, TestCaseScope scope)
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services =>
            {
                services.AddSingleton(new DeliveryRecorder<ReportGenerated>());
                services.AddKeyedSingleton(BillingConsumer.RecorderKey, new DeliveryRecorder<OrderPlaced>());
                services.AddKeyedSingleton(ShippingConsumer.RecorderKey, new DeliveryRecorder<OrderPlaced>());
                services.AddHealthChecks().AddYetAnotherBusWrapper();
                services.AddYetAnotherBusWrapper(
                    builder =>
                    {
                        builder.AddQueue<ReportGenerated>(QueueLogicalEntityName)
                            .Configure(
                                options =>
                                {
                                    options.ConnectionString = scope.ConnectionString;
                                    options.AdministrationConnectionString = scope.AdministrationConnectionString;
                                    options.QueueName = scope.GetQueueName(QueueLogicalEntityName);
                                    testCase.ConfigureQueue(options);
                                }
                            );
                        builder.AddQueueConsumer<RecordingConsumer<ReportGenerated>, ReportGenerated>();
                        builder.AddTopic<OrderPlaced>(TopicLogicalEntityName)
                            .Configure(
                                options =>
                                {
                                    options.ConnectionString = scope.ConnectionString;
                                    options.AdministrationConnectionString = scope.AdministrationConnectionString;
                                    options.TopicName = scope.GetTopicName(TopicLogicalEntityName);
                                    testCase.ConfigureTopic(options);
                                }
                            );
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

    private static void ConfigureSubscription(
        IHealthChecksTestCase testCase,
        string subscriptionLogicalName,
        SubscriptionOptions options
    )
    {
        options.SubscriptionName = subscriptionLogicalName;
        testCase.ConfigureSubscription(subscriptionLogicalName, options);
    }
}

internal record HealthChecksTestResult(
    int TestCaseId,
    HealthStatus? OverallStatus,
    List<HealthCheckEntryDto> Entries,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
