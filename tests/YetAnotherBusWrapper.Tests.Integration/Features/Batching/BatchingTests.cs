using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;
using YetAnotherBusWrapper.Tests.Integration.Common.Host;
using YetAnotherBusWrapper.Tests.Integration.Common.Logging;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;
using YetAnotherBusWrapper.Tests.Integration.Common.Services;
using YetAnotherBusWrapper.Tests.Integration.Common.TestCollections;
using YetAnotherBusWrapper.Tests.Integration.Features.Batching.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.Batching.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Batching;

[Collection(EmulatorTestCollection3.CollectionName)]
public class BatchingTests
{
    private const string Feature = "batching";
    private const string LogicalEntityName = "reports";

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public BatchingTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task Batching_ShouldSendCollectionInBatchesThatFit(IBatchingTestCase testCase)
    {
        BatchingTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<BatchingTestResult> RunAsync(IBatchingTestCase testCase, CancellationToken cancellationToken)
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string queueName = await scope.CreateQueueAsync(LogicalEntityName, cancellationToken);

        SendResult? sendResult = null;
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(scope, queueName);
            await host.StartAsync(cancellationToken);
            IMessageSender<ReportGenerated> sender =
                host.Services.GetRequiredService<IMessageSender<ReportGenerated>>();
            sendResult = await sender.SendAsync(testCase.Messages, cancellationToken: cancellationToken);
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        List<PeekedMessageDto> queuedMessages = await scope.Inspector.PeekQueueAsync(queueName, cancellationToken);

        return new BatchingTestResult(
            testCase.TestCaseId,
            testCase.Messages.Select(SentReportDto.From).ToList(),
            sendResult,
            queuedMessages.Select(QueuedReportDto.From).ToList(),
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static IHost BuildHost(TestCaseScope scope, string queueName)
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services => services.AddYetAnotherBusWrapper(
                builder => builder.AddQueue<ReportGenerated>(LogicalEntityName)
                    .Configure(
                        options =>
                        {
                            options.ConnectionString = scope.ConnectionString;
                            options.QueueName = queueName;
                        }
                    )
            )
        );
    }
}

internal record BatchingTestResult(
    int TestCaseId,
    List<SentReportDto> SentMessages,
    SendResult? SendResult,
    List<QueuedReportDto> QueuedMessages,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
