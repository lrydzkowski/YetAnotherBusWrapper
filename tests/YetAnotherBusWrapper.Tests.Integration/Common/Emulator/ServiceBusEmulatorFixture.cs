using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Testcontainers.ServiceBus;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Emulator;

public sealed class ServiceBusEmulatorFixture : IAsyncLifetime
{
    private const string EmulatorImage = "mcr.microsoft.com/azure-messaging/servicebus-emulator:2.0.1";

    private readonly ServiceBusContainer _container = new ServiceBusBuilder(EmulatorImage)
        .WithAcceptLicenseAgreement(true)
        .Build();

    private ServiceBusAdministrationClient? _administrationClient;
    private ServiceBusClient? _client;

    public string ConnectionString => _container.GetConnectionString();

    public string AdministrationConnectionString => _container.GetHttpConnectionString();

    public EmulatorInspector Inspector =>
        new(
            _administrationClient ?? throw new InvalidOperationException("The emulator has not been started."),
            _client ?? throw new InvalidOperationException("The emulator has not been started.")
        );

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        _administrationClient = new ServiceBusAdministrationClient(AdministrationConnectionString);
        _client = new ServiceBusClient(_container.GetConnectionString());
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
        {
            await _client.DisposeAsync();
        }

        await _container.DisposeAsync();
    }
}
