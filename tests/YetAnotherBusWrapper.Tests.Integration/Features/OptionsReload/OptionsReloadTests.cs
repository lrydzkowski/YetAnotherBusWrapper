using Microsoft.Extensions.Configuration;
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
using YetAnotherBusWrapper.Tests.Integration.Features.OptionsReload.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.OptionsReload.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsReload;

[Collection(EmulatorTestCollection2.CollectionName)]
public class OptionsReloadTests
{
    private const string Feature = "options-reload";
    private const string LogicalEntityName = "reports";

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(30);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public OptionsReloadTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
        _verifySettings.ScrubMember<MessageContext>(context => context.EnqueuedTime);
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task OptionsReload_ShouldKeepOptionsReadAtStartup(IOptionsReloadTestCase testCase)
    {
        OptionsReloadTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<OptionsReloadTestResult> RunAsync(
        IOptionsReloadTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        await testCase.PrepareAsync(scope, cancellationToken);
        DeliveryRecorder<ReportGenerated> recorder = new();
        IConfigurationRoot configuration = BuildConfiguration(testCase, scope);

        bool started = false;
        List<string>? queuesAfterStart = null;
        SendOutcomeDto? send = null;
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(configuration, scope, recorder);
            await host.StartAsync(cancellationToken);
            started = true;
            queuesAfterStart = await scope.GetQueueNamesAsync(cancellationToken);
            ReloadConfiguration(configuration, testCase.GetChangedOptions(scope));
            send = await SendOutcomeDto.CaptureAsync(
                () => host.Services.GetRequiredService<IMessageSender<ReportGenerated>>()
                    .SendAsync(new ReportGenerated(1, "report-1"), cancellationToken: cancellationToken)
            );
            await recorder.WaitForDeliveriesAsync(testCase.ExpectedDeliveries, DeliveryTimeout, cancellationToken);
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        return new OptionsReloadTestResult(
            testCase.TestCaseId,
            started,
            queuesAfterStart,
            send,
            recorder.GetDeliveries(),
            await scope.GetQueueNamesAsync(cancellationToken),
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static IConfigurationRoot BuildConfiguration(IOptionsReloadTestCase testCase, TestCaseScope scope)
    {
        Dictionary<string, string?> options = new()
        {
            ["ConnectionString"] = scope.ConnectionString,
            ["AdministrationConnectionString"] = scope.AdministrationConnectionString,
            ["QueueName"] = scope.GetQueueName(LogicalEntityName),
            ["CreateOnStartup"] = "true"
        };
        foreach ((string key, string? value) in testCase.GetInitialOptions(scope))
        {
            options[key] = value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(options.Select(option => KeyValuePair.Create(GetKey(option.Key), option.Value)))
            .Build();
    }

    private static void ReloadConfiguration(IConfigurationRoot configuration, Dictionary<string, string?> options)
    {
        foreach ((string key, string? value) in options)
        {
            configuration[GetKey(key)] = value;
        }

        configuration.Reload();
    }

    private static string GetKey(string optionName)
    {
        return $"{LogicalEntityName}:{optionName}";
    }

    private static IHost BuildHost(
        IConfiguration configuration,
        TestCaseScope scope,
        DeliveryRecorder<ReportGenerated> recorder
    )
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services =>
            {
                services.AddSingleton(recorder);
                services.AddYetAnotherBusWrapper(
                    builder =>
                    {
                        builder.AddQueue<ReportGenerated>(LogicalEntityName)
                            .Bind(configuration.GetSection(LogicalEntityName));
                        builder.AddQueueConsumer<RecordingConsumer<ReportGenerated>, ReportGenerated>();
                    }
                );
            }
        );
    }
}

internal record OptionsReloadTestResult(
    int TestCaseId,
    bool Started,
    List<string>? QueuesAfterStart,
    SendOutcomeDto? Send,
    List<RecordedDelivery<ReportGenerated>> Deliveries,
    List<string> QueuesAfterStop,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
