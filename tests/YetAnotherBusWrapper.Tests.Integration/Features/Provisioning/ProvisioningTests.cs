using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Hosting;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;
using YetAnotherBusWrapper.Tests.Integration.Common.Host;
using YetAnotherBusWrapper.Tests.Integration.Common.Logging;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;
using YetAnotherBusWrapper.Tests.Integration.Common.Services;
using YetAnotherBusWrapper.Tests.Integration.Common.TestCollections;
using YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning;

[Collection(EmulatorTestCollection2.CollectionName)]
public class ProvisioningTests
{
    private const string Feature = "provisioning";
    private const string LogicalEntityName = "orders";
    private const int ExistingQueueMaxDeliveryCount = 3;

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public ProvisioningTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task Provisioning_ShouldCreateAndDeleteQueuesFromOptions(IProvisioningTestCase testCase)
    {
        ProvisioningTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<ProvisioningTestResult> RunAsync(
        IProvisioningTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string queueName = scope.GetQueueName(LogicalEntityName);
        await PrepareExistingQueueAsync(testCase, scope, queueName, cancellationToken);
        CreateOptionsRecorder createOptionsRecorder = new(testCase, scope.Inspector);

        bool started = false;
        bool stopped = false;
        QueuePropertiesDto? queueAfterStart = null;
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, queueName, createOptionsRecorder);
            await host.StartAsync(cancellationToken);
            started = true;
            queueAfterStart = await scope.Inspector.GetQueueAsync(queueName, cancellationToken);
            await host.StopAsync(cancellationToken);
            stopped = true;
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        QueuePropertiesDto? queueAfterStop = await scope.Inspector.GetQueueAsync(queueName, cancellationToken);

        return new ProvisioningTestResult(
            testCase.TestCaseId,
            started,
            stopped,
            createOptionsRecorder.CreateOptions,
            queueAfterStart,
            queueAfterStop,
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static async Task PrepareExistingQueueAsync(
        IProvisioningTestCase testCase,
        TestCaseScope scope,
        string queueName,
        CancellationToken cancellationToken
    )
    {
        if (!testCase.QueueExistsBeforeStart)
        {
            return;
        }

        await scope.Inspector.CreateQueueAsync(
            new CreateQueueOptions(queueName) { MaxDeliveryCount = ExistingQueueMaxDeliveryCount },
            cancellationToken
        );
    }

    private static IHost BuildHost(
        IProvisioningTestCase testCase,
        TestCaseScope scope,
        string queueName,
        CreateOptionsRecorder createOptionsRecorder
    )
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services => services.AddYetAnotherBusWrapper(
                builder => builder.AddQueue<OrderPlaced>(LogicalEntityName, createOptionsRecorder.Configure)
                    .Configure(
                        options =>
                        {
                            options.ConnectionString = scope.ConnectionString;
                            options.AdministrationConnectionString = scope.AdministrationConnectionString;
                            options.QueueName = queueName;
                            testCase.ConfigureQueue(options);
                        }
                    )
            )
        );
    }

    private sealed class CreateOptionsRecorder
    {
        private readonly EmulatorInspector _inspector;
        private readonly IProvisioningTestCase _testCase;

        public CreateOptionsRecorder(IProvisioningTestCase testCase, EmulatorInspector inspector)
        {
            _testCase = testCase;
            _inspector = inspector;
        }

        public CreateQueueOptionsDto? CreateOptions { get; private set; }

        public void Configure(CreateQueueOptions options)
        {
            _testCase.ConfigureCreateOptions(options);
            CreateOptions = CreateQueueOptionsDto.From(options);
            if (_testCase.QueueCreatedDuringProvisioning)
            {
                _inspector.CreateQueueAsync(options.Name, CancellationToken.None).GetAwaiter().GetResult();
            }
        }
    }
}

internal record ProvisioningTestResult(
    int TestCaseId,
    bool Started,
    bool Stopped,
    CreateQueueOptionsDto? CreateOptions,
    QueuePropertiesDto? QueueAfterStart,
    QueuePropertiesDto? QueueAfterStop,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
