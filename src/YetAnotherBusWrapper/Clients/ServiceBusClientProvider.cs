#region

using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using YetAnotherBusWrapper.Options;

#endregion

namespace YetAnotherBusWrapper.Clients;

internal sealed class ServiceBusClientProvider : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, Lazy<ServiceBusAdministrationClient>> _administrationClients =
        new(StringComparer.Ordinal);

    private readonly ConcurrentDictionary<string, Lazy<ServiceBusClient>> _clients =
        new(StringComparer.Ordinal);

    private readonly ClientRetryOptionsValidator _retryOptionsValidator;

    public ServiceBusClientProvider(ClientRetryOptionsValidator retryOptionsValidator)
    {
        _retryOptionsValidator = retryOptionsValidator;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (Lazy<ServiceBusClient> client in _clients.Values.Where(client => client.IsValueCreated))
        {
            await client.Value.DisposeAsync();
        }

        _clients.Clear();
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public ServiceBusClient GetClient(string connectionString, ClientRetryOptions retryOptions)
    {
        return _clients.GetOrAdd(
                connectionString,
                key => new Lazy<ServiceBusClient>(() => CreateClient(key, retryOptions))
            )
            .Value;
    }

    public ServiceBusAdministrationClient GetAdministrationClient(string connectionString)
    {
        return _administrationClients.GetOrAdd(
                connectionString,
                key => new Lazy<ServiceBusAdministrationClient>(() => new ServiceBusAdministrationClient(key))
            )
            .Value;
    }

    public ServiceBusAdministrationClient GetAdministrationClient(
        string connectionString,
        string? administrationConnectionString
    )
    {
        return GetAdministrationClient(
            string.IsNullOrWhiteSpace(administrationConnectionString)
                ? connectionString
                : administrationConnectionString
        );
    }

    private ServiceBusClient CreateClient(string connectionString, ClientRetryOptions retryOptions)
    {
        _retryOptionsValidator.EnsureConsistentRetryOptions();
        ServiceBusClientOptions options = new()
        {
            RetryOptions = new ServiceBusRetryOptions
            {
                MaxRetries = retryOptions.MaxRetries,
                TryTimeout = retryOptions.TryTimeout,
                MaxDelay = retryOptions.MaxDelay
            }
        };

        return new ServiceBusClient(connectionString, options);
    }
}
