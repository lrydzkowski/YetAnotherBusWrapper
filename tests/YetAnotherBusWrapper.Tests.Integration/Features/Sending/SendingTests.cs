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
using YetAnotherBusWrapper.Tests.Integration.Features.Sending.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.Sending.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sending;

[Collection(EmulatorTestCollection3.CollectionName)]
public class SendingTests
{
    private const string Feature = "sending";
    private const string LogicalEntityName = "orders";

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public SendingTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task Sending_ShouldPutSerializedMessageOnQueue(ISendingTestCase testCase)
    {
        SendingTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<SendingTestResult> RunAsync(ISendingTestCase testCase, CancellationToken cancellationToken)
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string queueName = await scope.CreateQueueAsync(LogicalEntityName, cancellationToken);

        SendResult? sendResult = null;
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, queueName);
            await host.StartAsync(cancellationToken);
            IMessageSender<OrderPlaced> sender = host.Services.GetRequiredService<IMessageSender<OrderPlaced>>();
            sendResult = await sender.SendAsync(testCase.Message, cancellationToken: cancellationToken);
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        List<PeekedMessageDto> queuedMessages = await scope.Inspector.PeekQueueAsync(queueName, cancellationToken);

        return new SendingTestResult(
            testCase.TestCaseId,
            testCase.Message,
            sendResult,
            queuedMessages,
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static IHost BuildHost(ISendingTestCase testCase, TestCaseScope scope, string queueName)
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services => services.AddYetAnotherBusWrapper(
                builder =>
                {
                    builder.AddQueue<OrderPlaced>(LogicalEntityName, configureMessage: testCase.ConfigureMessage)
                        .Configure(
                            options =>
                            {
                                options.ConnectionString = scope.ConnectionString;
                                options.QueueName = queueName;
                            }
                        );
                    testCase.ConfigureBus(builder);
                }
            )
        );
    }
}

internal record SendingTestResult(
    int TestCaseId,
    OrderPlaced SentMessage,
    SendResult? SendResult,
    List<PeekedMessageDto> QueuedMessages,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
