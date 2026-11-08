using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Lifecycle.Data;

public sealed class LifecycleDriver
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IHost _host;
    private readonly string _queueName;
    private readonly TestCaseScope _scope;
    private readonly List<SendOutcomeDto> _sendOutcomes = [];

    public LifecycleDriver(IHost host, TestCaseScope scope, string queueName)
    {
        _host = host;
        _scope = scope;
        _queueName = queueName;
    }

    public int SentMessageCount { get; private set; }

    public Task StartHostAsync(CancellationToken cancellationToken)
    {
        return _host.StartAsync(cancellationToken);
    }

    public Task StopHostAsync(CancellationToken cancellationToken)
    {
        return _host.StopAsync(cancellationToken);
    }

    public async Task StartHostedServicesAsync(CancellationToken cancellationToken)
    {
        foreach (IHostedService hostedService in _host.Services.GetServices<IHostedService>())
        {
            await hostedService.StartAsync(cancellationToken);
        }
    }

    public async Task StopHostedServicesAsync(CancellationToken cancellationToken)
    {
        foreach (IHostedService hostedService in _host.Services.GetServices<IHostedService>())
        {
            await hostedService.StopAsync(cancellationToken);
        }
    }

    public async Task DisposeHostAsync()
    {
        if (_host is IAsyncDisposable asyncDisposableHost)
        {
            await asyncDisposableHost.DisposeAsync();

            return;
        }

        _host.Dispose();
    }

    public void DisposeServicesSynchronously()
    {
        ((IDisposable)_host.Services).Dispose();
    }

    public async Task SendThroughSenderAsync(OrderPlaced message, CancellationToken cancellationToken)
    {
        IMessageSender<OrderPlaced> sender = _host.Services.GetRequiredService<IMessageSender<OrderPlaced>>();
        SendOutcomeDto outcome = await SendOutcomeDto.CaptureAsync(
            () => sender.SendAsync(message, cancellationToken: cancellationToken)
        );
        _sendOutcomes.Add(outcome);
        SentMessageCount++;
    }

    public async Task SendDirectlyAsync(OrderPlaced message, CancellationToken cancellationToken)
    {
        ServiceBusMessage serviceBusMessage = new(BinaryData.FromObjectAsJson(message, JsonSerializerOptions))
        {
            ContentType = "application/json"
        };
        await _scope.Inspector.SendToQueueAsync(_queueName, serviceBusMessage, cancellationToken);
        SentMessageCount++;
    }

    internal List<SendOutcomeDto> GetSendOutcomes()
    {
        return _sendOutcomes.ToList();
    }
}
