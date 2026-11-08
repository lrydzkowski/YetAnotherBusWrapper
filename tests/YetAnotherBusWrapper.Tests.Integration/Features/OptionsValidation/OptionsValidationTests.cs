using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;
using YetAnotherBusWrapper.Tests.Integration.Common.Host;
using YetAnotherBusWrapper.Tests.Integration.Common.Logging;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;
using YetAnotherBusWrapper.Tests.Integration.Common.Services;
using YetAnotherBusWrapper.Tests.Integration.Common.TestCollections;
using YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation;

[Collection(EmulatorTestCollection4.CollectionName)]
public class OptionsValidationTests
{
    private const string Feature = "options-validation";
    private const string LogicalEntityName = "orders";
    private const int RegisteredServicesTestCaseId = 4;

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public OptionsValidationTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task OptionsValidation_ShouldValidateEnabledQueuesOnStart(IOptionsValidationTestCase testCase)
    {
        OptionsValidationTestResult result = await RunAsync(
            testCase.TestCaseId,
            scope => BuildHost(testCase, scope),
            TestContext.Current.CancellationToken
        );

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    [Fact]
    public async Task OptionsValidation_ShouldAcceptValuesFromRegisteredServices()
    {
        OptionsValidationTestResult result = await RunAsync(
            RegisteredServicesTestCaseId,
            BuildHostWithValuesFromRegisteredServices,
            TestContext.Current.CancellationToken
        );

        await Verify(result, _verifySettings);
    }

    private async Task<OptionsValidationTestResult> RunAsync(
        int testCaseId,
        Func<TestCaseScope, IHost> buildHost,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCaseId);

        bool started = false;
        ThrownException? thrownException = null;
        try
        {
            using IHost host = buildHost(scope);
            await host.StartAsync(cancellationToken);
            started = true;
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        return new OptionsValidationTestResult(testCaseId, started, scope.Logs.GetLogs(), thrownException);
    }

    private static IHost BuildHost(IOptionsValidationTestCase testCase, TestCaseScope scope)
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services => services.AddYetAnotherBusWrapper(
                builder => builder.AddQueue<OrderPlaced>(LogicalEntityName)
                    .Configure(options => testCase.ConfigureQueue(options, scope))
            )
        );
    }

    private static IHost BuildHostWithValuesFromRegisteredServices(TestCaseScope scope)
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services =>
            {
                services.Configure<QueueConnectionValues>(
                    values =>
                    {
                        values.ConnectionString = scope.ConnectionString;
                        values.QueueName = scope.GetQueueName(LogicalEntityName);
                    }
                );
                services.AddYetAnotherBusWrapper(
                    builder => builder.AddQueue<OrderPlaced>(LogicalEntityName)
                        .Configure<IOptions<QueueConnectionValues>>(
                            (options, values) =>
                            {
                                options.ConnectionString = values.Value.ConnectionString;
                                options.QueueName = values.Value.QueueName;
                            }
                        )
                );
            }
        );
    }
}

internal class QueueConnectionValues
{
    public string? ConnectionString { get; set; }

    public string? QueueName { get; set; }
}

internal record OptionsValidationTestResult(
    int TestCaseId,
    bool Started,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
