using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;
using YetAnotherBusWrapper.Tests.Integration.Common.Host;
using YetAnotherBusWrapper.Tests.Integration.Common.Logging;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;
using YetAnotherBusWrapper.Tests.Integration.Common.Services;
using YetAnotherBusWrapper.Tests.Integration.Common.TestCollections;
using YetAnotherBusWrapper.Tests.Integration.Features.EntityNames.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.EntityNames.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.EntityNames;

[Collection(EmulatorTestCollection4.CollectionName)]
public class EntityNamesTests
{
    private const string Feature = "entity-names";
    private const string LogicalEntityName = "orders";

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(30);
    private static readonly OrderPlaced Message = new("order-001", 1, OrderStatus.New);

    private readonly ServiceBusEmulatorFixture _emulator;
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public EntityNamesTests(ServiceBusEmulatorFixture emulator)
    {
        _emulator = emulator;
    }

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task EntityNames_ShouldUseResolvedQueueName(IEntityNamesTestCase testCase)
    {
        EntityNamesTestResult result = await RunAsync(testCase, TestContext.Current.CancellationToken);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private async Task<EntityNamesTestResult> RunAsync(
        IEntityNamesTestCase testCase,
        CancellationToken cancellationToken
    )
    {
        await using TestCaseScope scope = new(_emulator, Feature, testCase.TestCaseId);
        string configuredQueueName = scope.GetQueueName(testCase.QueueName);
        DeliveryRecorder<OrderPlaced> recorder = new();

        List<string> queueNamesAfterStart = [];
        ThrownException? thrownException = null;
        try
        {
            using IHost host = BuildHost(testCase, scope, configuredQueueName, recorder);
            await host.StartAsync(cancellationToken);
            queueNamesAfterStart = await scope.GetQueueNamesAsync(cancellationToken);
            IMessageSender<OrderPlaced> sender = host.Services.GetRequiredService<IMessageSender<OrderPlaced>>();
            await sender.SendAsync(Message, cancellationToken: cancellationToken);
            await recorder.WaitForDeliveriesAsync(1, DeliveryTimeout, cancellationToken);
            await host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        List<string> queueNamesAfterStop = await scope.GetQueueNamesAsync(cancellationToken);

        return new EntityNamesTestResult(
            testCase.TestCaseId,
            configuredQueueName,
            testCase.InstanceName,
            testCase.UseInstanceSuffix,
            queueNamesAfterStart,
            recorder.GetDeliveries().Select(delivery => delivery.Message).ToList(),
            queueNamesAfterStop,
            scope.Logs.GetLogs(),
            thrownException
        );
    }

    private static IHost BuildHost(
        IEntityNamesTestCase testCase,
        TestCaseScope scope,
        string configuredQueueName,
        DeliveryRecorder<OrderPlaced> recorder
    )
    {
        IInstanceNameProvider instanceNameProvider = Substitute.For<IInstanceNameProvider>();
        instanceNameProvider.GetInstanceName().Returns(testCase.InstanceName);

        return TestHostBuilder.Build(
            scope.Logs,
            services =>
            {
                services.AddSingleton(recorder);
                services.AddYetAnotherBusWrapper(
                    builder =>
                    {
                        builder.UseInstanceNameProvider(instanceNameProvider);
                        builder.AddQueue<OrderPlaced>(LogicalEntityName)
                            .Configure(
                                options =>
                                {
                                    options.ConnectionString = scope.ConnectionString;
                                    options.AdministrationConnectionString = scope.AdministrationConnectionString;
                                    options.QueueName = configuredQueueName;
                                    options.UseInstanceSuffix = testCase.UseInstanceSuffix;
                                    options.CreateOnStartup = true;
                                    options.DeleteOnShutdown = true;
                                }
                            );
                        builder.AddQueueConsumer<RecordingConsumer<OrderPlaced>, OrderPlaced>();
                    }
                );
            }
        );
    }
}

internal record EntityNamesTestResult(
    int TestCaseId,
    string ConfiguredQueueName,
    string InstanceName,
    bool UseInstanceSuffix,
    List<string> QueueNamesAfterStart,
    List<OrderPlaced> DeliveredMessages,
    List<string> QueueNamesAfterStop,
    List<CapturedLogDto> Logs,
    ThrownException? ThrownException
);
