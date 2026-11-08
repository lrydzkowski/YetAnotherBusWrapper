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
using YetAnotherBusWrapper.Tests.Integration.Features.BestEffortSending.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.BestEffortSending.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.BestEffortSending;

[Collection(EmulatorTestCollection2.CollectionName)]
public class BestEffortSendingTests
{
    private const string Feature = "best-effort-sending";
    private const string QueueLogicalEntityName = "reports";
    private const string TopicLogicalEntityName = "orders";

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public BestEffortSendingTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task BestEffortSending_ShouldReturnSkippedOrFailedInsteadOfThrowing(
        IBestEffortSendingTestCase testCase
    )
    {
        BestEffortSendingTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<BestEffortSendingTestResult> RunAsync(
        IBestEffortSendingTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string queueName = await PrepareQueueAsync(testCase, scope, cancellationToken);

        SendOutcomeDto? queueSend = null;
        SendOutcomeDto? topicSend = null;
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, queueName);
            await host.StartAsync(cancellationToken);
            queueSend = await SendAsync(host, testCase.QueueMessages, cancellationToken);
            topicSend = await SendAsync(host, testCase.TopicMessages, cancellationToken);
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        List<PeekedMessageDto> queuedMessages = testCase.QueueExists
            ? await scope.Inspector.PeekQueueAsync(queueName, cancellationToken)
            : [];

        return new BestEffortSendingTestResult(
            testCase.TestCaseId,
            queueSend,
            topicSend,
            queuedMessages.Select(message => message.MessageId).ToList(),
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static async Task<string> PrepareQueueAsync(
        IBestEffortSendingTestCase testCase,
        TestCaseScope scope,
        CancellationToken cancellationToken
    )
    {
        if (!testCase.QueueExists)
        {
            return scope.GetQueueName(QueueLogicalEntityName);
        }

        return await scope.CreateQueueAsync(QueueLogicalEntityName, cancellationToken);
    }

    private static async Task<SendOutcomeDto?> SendAsync<TMessage>(
        IHost host,
        IReadOnlyList<TMessage> messages,
        CancellationToken cancellationToken
    )
    {
        if (messages.Count == 0)
        {
            return null;
        }

        IMessageSender<TMessage> sender = host.Services.GetRequiredService<IMessageSender<TMessage>>();

        return await SendOutcomeDto.CaptureAsync(
            () => sender.SendAsync(messages, cancellationToken: cancellationToken)
        );
    }

    private static IHost BuildHost(IBestEffortSendingTestCase testCase, TestCaseScope scope, string queueName)
    {
        return TestHostBuilder.Build(
            scope.Logs,
            services => services.AddYetAnotherBusWrapper(
                builder =>
                {
                    builder.AddQueue<ReportGenerated>(QueueLogicalEntityName)
                        .Configure(
                            options =>
                            {
                                options.ConnectionString = scope.ConnectionString;
                                options.QueueName = queueName;
                                options.BestEffort = true;
                                testCase.ConfigureQueue(options);
                            }
                        );
                    builder.AddTopic<OrderPlaced>(TopicLogicalEntityName)
                        .Configure(
                            options =>
                            {
                                options.ConnectionString = scope.ConnectionString;
                                options.TopicName = scope.GetTopicName(TopicLogicalEntityName);
                                options.BestEffort = true;
                                testCase.ConfigureTopic(options);
                            }
                        );
                }
            )
        );
    }
}

internal record BestEffortSendingTestResult(
    int TestCaseId,
    SendOutcomeDto? QueueSend,
    SendOutcomeDto? TopicSend,
    List<string> QueuedMessageIds,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
