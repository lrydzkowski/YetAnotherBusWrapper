using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Sending;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;
using YetAnotherBusWrapper.Tests.Integration.Common.Host;
using YetAnotherBusWrapper.Tests.Integration.Common.Logging;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;
using YetAnotherBusWrapper.Tests.Integration.Common.Services;
using YetAnotherBusWrapper.Tests.Integration.Common.TestCollections;
using YetAnotherBusWrapper.Tests.Integration.Features.DuplicateDetection.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.DuplicateDetection.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.DuplicateDetection;

[Collection(EmulatorTestCollection4.CollectionName)]
public class DuplicateDetectionTests
{
    private const string Feature = "duplicate-detection";
    private const string LogicalEntityName = "orders";

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public DuplicateDetectionTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task DuplicateDetection_ShouldDropMessagesWithRepeatedMessageId(IDuplicateDetectionTestCase testCase)
    {
        DuplicateDetectionTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<DuplicateDetectionTestResult> RunAsync(
        IDuplicateDetectionTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string queueName = scope.GetQueueName(LogicalEntityName);

        List<SendResult> sendResults = [];
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, queueName);
            await host.StartAsync(cancellationToken);
            IMessageSender<OrderPlaced> sender = host.Services.GetRequiredService<IMessageSender<OrderPlaced>>();
            foreach (IReadOnlyList<OrderPlaced> messages in testCase.SendCalls)
            {
                sendResults.Add(await sender.SendAsync(messages, cancellationToken: cancellationToken));
            }

            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        QueuePropertiesDto? queue = await scope.Inspector.GetQueueAsync(queueName, cancellationToken);
        List<PeekedMessageDto> queuedMessages = await scope.Inspector.PeekQueueAsync(queueName, cancellationToken);

        return new DuplicateDetectionTestResult(
            testCase.TestCaseId,
            testCase.SendCalls,
            sendResults,
            queue,
            queuedMessages,
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static IHost BuildHost(IDuplicateDetectionTestCase testCase, TestCaseScope scope, string queueName)
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services => services.AddYetAnotherBusWrapper(
                builder => builder.AddQueue<OrderPlaced>(LogicalEntityName, configureMessage: SetMessageId)
                    .Configure(
                        options =>
                        {
                            options.ConnectionString = scope.ConnectionString;
                            options.AdministrationConnectionString = scope.AdministrationConnectionString;
                            options.QueueName = queueName;
                            options.CreateOnStartup = true;
                            options.FailStartupOnProvisioningError = true;
                            options.DuplicateDetectionWindow = testCase.DuplicateDetectionWindow;
                        }
                    )
            )
        );
    }

    private static void SetMessageId(ServiceBusMessage serviceBusMessage, OrderPlaced message)
    {
        serviceBusMessage.MessageId = MessageIdSanitizer.Sanitize(message.OrderId);
    }
}

internal record DuplicateDetectionTestResult(
    int TestCaseId,
    IReadOnlyList<IReadOnlyList<OrderPlaced>> SendCalls,
    List<SendResult> SendResults,
    QueuePropertiesDto? Queue,
    List<PeekedMessageDto> QueuedMessages,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
