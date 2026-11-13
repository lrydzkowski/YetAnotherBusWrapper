using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
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
    public async Task OptionsValidation_ShouldValidateOptionsOnStart(IOptionsValidationTestCase testCase)
    {
        OptionsValidationTestResult result = await RunAsync(
            testCase.TestCaseId,
            scope => BuildHost(testCase, scope),
            TestContext.Current.CancellationToken
        );

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
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
            services =>
            {
                services.AddSingleton(new DeliveryRecorder<OrderPlaced>());
                services.AddYetAnotherBusWrapper(builder => testCase.Configure(builder, scope));
            }
        );
    }
}

internal record OptionsValidationTestResult(
    int TestCaseId,
    bool Started,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
