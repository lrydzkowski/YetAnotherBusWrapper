# YetAnotherBusWrapper

YetAnotherBusWrapper connects ASP.NET Core applications to Azure Service Bus.
An application registers each queue, topic, and subscription once. It then
sends messages through a typed sender and receives them through a typed
consumer, and neither type depends on the Azure SDK. The library provisions
entities, starts and stops consumers, shares connections, and reports health.

The library never reads settings from `IConfiguration`,
`appsettings.json`, or environment variables. The host fills the library's
options in whatever way it wants.

## Contents

- [Packages](#packages)
- [Quick start](#quick-start)
- [Registration](#registration)
- [Sending](#sending)
- [Consuming](#consuming)
- [Retries and the dead-letter queue](#retries-and-the-dead-letter-queue)
- [Provisioning](#provisioning)
- [Entity names and the instance suffix](#entity-names-and-the-instance-suffix)
- [Disabled entities](#disabled-entities)
- [Best-effort senders](#best-effort-senders)
- [Clients and lifecycle](#clients-and-lifecycle)
- [Health checks](#health-checks)
- [Per-instance broadcast subscriptions](#per-instance-broadcast-subscriptions)
- [Keeping core code independent of Service Bus](#keeping-core-code-independent-of-service-bus)
- [Sample application](#sample-application)

## Packages

All packages target `net10.0`.

- `YetAnotherBusWrapper.Abstractions`: the types application core code depends
  on: `IMessageSender<TMessage>`, `IMessageConsumer<TMessage>`,
  `MessageContext`, `SendOptions`, `SendResult`, and `SendStatus`. It has no
  Azure SDK reference.
- `YetAnotherBusWrapper`: registration, clients, senders, provisioning, hosted
  consumers, serialization, entity names, and health checks.

The packages are not published to a feed. Build them with `dotnet pack` and
add the output folder as a local package source.

## Quick start

The examples use these message types:

```csharp
public sealed record OrderPlaced(string OrderId, decimal Total);

public sealed record PriceChanged(string ProductId, decimal Price);
```

A consumer implements one method:

```csharp
using YetAnotherBusWrapper.Abstractions;

public sealed class OrderPlacedConsumer : IMessageConsumer<OrderPlaced>
{
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger)
    {
        _logger = logger;
    }

    public Task ConsumeAsync(
        OrderPlaced message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Order {OrderId} placed", message.OrderId);

        return Task.CompletedTask;
    }
}
```

The host registers the queue and its consumer, and binds their options from
its own configuration:

```json
{
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://example.servicebus.windows.net/;...",
    "Orders": {
      "QueueName": "orders",
      "CreateOnStartup": true,
      "MaxDeliveryCount": 5,
      "Consumer": {
        "MaxConcurrentCalls": 4
      }
    }
  }
}
```

```csharp
using YetAnotherBusWrapper;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
IConfigurationSection serviceBus = builder.Configuration.GetSection(
    "ServiceBus"
);
string? connectionString = serviceBus["ConnectionString"];

builder.Services.AddYetAnotherBusWrapper(bus =>
{
    bus.AddQueue<OrderPlaced>("orders")
        .Configure(options => options.ConnectionString = connectionString)
        .Bind(serviceBus.GetSection("Orders"));
    bus.AddQueueConsumer<OrderPlacedConsumer, OrderPlaced>()
        .Bind(serviceBus.GetSection("Orders:Consumer"));
});

WebApplication app = builder.Build();
app.Run();
```

Any class can now send an `OrderPlaced` message through
`IMessageSender<OrderPlaced>`, and `OrderPlacedConsumer` receives it.

## Registration

`AddYetAnotherBusWrapper` on the service collection is the single entry point.
Its callback receives a `YetAnotherBusWrapperBuilder`. Calling
`AddYetAnotherBusWrapper` more than once adds to the same registrations.

### Queues

`AddQueue<TMessage>(logicalEntityName)` registers a queue for a message type
under a logical entity name, and registers `IMessageSender<TMessage>` for
it. The logical entity name is the name the application uses in its own
options. The entity name in the namespace comes from `QueueName`.

`AddQueueConsumer<TConsumer, TMessage>()` registers a consumer for the queue
of `TMessage`. The queue must be registered first. A queue has at most one
consumer.

### Topics and subscriptions

`AddTopic<TMessage>(logicalEntityName)` registers a topic and its sender.

`AddTopicConsumer<TConsumer, TMessage>(subscriptionLogicalName)` registers
a consumer together with its own subscription on the topic of `TMessage`.
Register a consumer once per subscription to handle the same message type
independently in several consumers:

```csharp
builder.Services.AddYetAnotherBusWrapper(bus =>
{
    bus.AddTopic<PriceChanged>("prices")
        .Configure(options => options.ConnectionString = connectionString)
        .Bind(serviceBus.GetSection("Prices"));
    bus.AddTopicConsumer<PriceCacheConsumer, PriceChanged>("cache")
        .Bind(serviceBus.GetSection("Prices:Cache"));
    bus.AddTopicConsumer<PriceSearchIndexConsumer, PriceChanged>("search")
        .Bind(serviceBus.GetSection("Prices:Search"));
});
```

A subscription has no connection string of its own. It uses the
connection of its topic.

### Options

Each registration returns an `OptionsBuilder<T>` for its options type, so
the host decides where values come from. Use `Configure`, `Bind`,
`BindConfiguration`, `PostConfigure`, or any other options builder method.
Options are named options, and the name is the logical entity name. The
logical entity name of a subscription combines the topic's logical entity
name and the name passed to `AddTopicConsumer`, for example
`prices/cache`.

Shared values, such as the connection string, can come from one place and
entity values from another, as in the examples above. Options registered
later override earlier ones, so a `Bind` after `Configure` lets an entity
section override the shared connection string.

The library reads each entity's options once, when the host starts, and
keeps them until the app stops. Configuration reloads, such as an edited
`appsettings.json`, are not applied. Restart the app to apply new options.

To fill the options from environment variables, read them in your own code:

```csharp
bus.AddQueue<OrderPlaced>("orders")
    .Configure(options =>
    {
        options.ConnectionString = Environment.GetEnvironmentVariable(
            "ServiceBusConnection"
        );
        options.QueueName = Environment.GetEnvironmentVariable(
            "OrdersQueueName"
        );
    });
```

The callback passed to `AddYetAnotherBusWrapper` runs during registration, so it
has no service provider. To fill the options from registered services, use
`Configure<TDep>` or `PostConfigure<TDep>`. These run later and receive the
service:

```csharp
bus.AddQueue<OrderPlaced>("orders")
    .Configure<IOptions<ConnectionStringsOptions>>(
        (options, connectionStrings) =>
            options.ConnectionString = connectionStrings.Value.ServiceBus
    );
```

A scoped service cannot be used as `TDep` directly.

Queue, queue consumer, topic, and subscription options are validated when
the host starts. An enabled queue needs `ConnectionString` and `QueueName`,
an enabled topic needs `ConnectionString` and `TopicName`, and an enabled
subscription needs `SubscriptionName`. A missing value fails startup with a
message that names the logical entity name and the missing option. Disabled
entities skip validation, and so do the consumers of a disabled queue and
the subscriptions of a disabled topic.

Numeric and time options are validated at the same time, including options
that the current setup does not use. A value outside its range fails startup
with a message that names the logical entity name, the option, and the
allowed range:

- `MaxDeliveryCount`: at least `1`.
- `DuplicateDetectionWindow`: zero, or between 20 seconds and 7 days.
- `ClientRetry.MaxRetries`: between `0` and `100`.
- `ClientRetry.TryTimeout`: between zero and 1 hour.
- `ClientRetry.MaxDelay`: zero or more.
- `MaxConcurrentCalls` and `MaxConcurrentSessions`: at least `1`.
- `PrefetchCount`: zero or more.
- `MaxAutoLockRenewalDuration`: zero or more, or
  `Timeout.InfiniteTimeSpan`.
- `SessionIdleTimeout`: unset, or greater than zero.

When more than one registration fails validation, startup throws an
`AggregateException` that holds one `OptionsValidationException` for each
of them.

`QueueOptions`:

- `Enabled` (default `true`).
- `ConnectionString`.
- `AdministrationConnectionString`: optional. When set, provisioning and
  existence health checks use it instead of `ConnectionString`.
- `QueueName`.
- `UseInstanceSuffix` (default `false`).
- `CreateOnStartup` (default `false`).
- `DeleteOnShutdown` (default `false`).
- `FailStartupOnProvisioningError` (default `false`).
- `MaxDeliveryCount` (default `10`). Applied when the library creates the
  queue.
- `DuplicateDetectionWindow` (default zero, which turns duplicate detection
  off). Applied when the library creates the queue.
- `BestEffort` (default `false`).
- `HealthCheck`: `Existence` (default) or `Peek`.
- `ClientRetry`: `MaxRetries` (default `2`), `TryTimeout` (default 10
  seconds), and `MaxDelay` (default 2 seconds).

`TopicOptions` has the same options as `QueueOptions`, with `TopicName`
instead of `QueueName`, and without `MaxDeliveryCount` and `HealthCheck`.
A topic health check always checks existence.

`SubscriptionOptions`:

- `Enabled` (default `true`).
- `SubscriptionName`.
- `UseInstanceSuffix`, `CreateOnStartup`, `DeleteOnShutdown`, and
  `FailStartupOnProvisioningError` (all default `false`).
- `MaxDeliveryCount` (default `10`). Applied when the library creates the
  subscription.
- `HealthCheck`: `Existence` (default) or `Peek`.
- The consumer processing options listed in
  [Processing options](#processing-options).

`QueueConsumerOptions`, returned by `AddQueueConsumer`, holds the consumer
processing options for a queue consumer.

### Registration rules

A message type belongs to exactly one queue or topic. These registrations
throw `InvalidOperationException` immediately, inside the
`AddYetAnotherBusWrapper` callback:

- A second queue or topic for a message type that is already registered.
- A second queue with the logical entity name of a registered queue, or a
  second topic with the logical entity name of a registered topic.
- A queue consumer for a message type without a queue, or with a topic.
- A topic consumer for a message type without a topic, or with a queue.
- A second queue consumer for a queue that already has one.
- The same subscription name passed to `AddTopicConsumer` twice for one
  topic.

### Serialization

Messages are serialized as JSON with System.Text.Json, using camelCase
property names and enums as strings. The content type is set to
`application/json`. Change the JSON settings once for all senders and
consumers:

```csharp
builder.Services.AddYetAnotherBusWrapper(bus =>
{
    bus.ConfigureJsonSerializerOptions(options =>
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    );
});
```

To use another format, implement `IMessageSerializer` and register it with
`bus.UseMessageSerializer<TSerializer>()`. The serializer is
registered as a singleton.

## Sending

Inject `IMessageSender<TMessage>` and send a message:

```csharp
using YetAnotherBusWrapper.Abstractions;

public sealed class OrderService
{
    private readonly IMessageSender<OrderPlaced> _sender;

    public OrderService(IMessageSender<OrderPlaced> sender)
    {
        _sender = sender;
    }

    public async Task<string> PlaceAsync(
        string orderId,
        decimal total,
        CancellationToken cancellationToken
    )
    {
        SendResult result = await _sender.SendAsync(
            new OrderPlaced(orderId, total),
            cancellationToken: cancellationToken
        );

        return result.MessageIds[0];
    }
}
```

`SendResult` holds a `Status` (`Sent`, `Skipped`, or `Failed`) and the IDs
of the messages that were sent. A normal sender only returns `Sent`.
`Skipped` and `Failed` come from [best-effort
senders](#best-effort-senders).

### Collections

Pass an `IReadOnlyCollection<TMessage>` to send several messages in one
call. Messages are added to a batch until it is full. The batch is sent,
and the remaining messages continue in a new batch. A message that doesn't
fit into an empty batch throws `InvalidOperationException` with the
message's position in the collection. A best-effort sender returns
`Failed` instead.

A send split into several batches is not atomic. If a later batch fails,
the batches sent before it stay sent.

### Scheduled enqueue

`SendOptions.ScheduledEnqueueTime` schedules every message in the call.
Service Bus delivers them when they are due:

```csharp
await _sender.SendAsync(
    message,
    new SendOptions
    {
        ScheduledEnqueueTime = _timeProvider.GetUtcNow().AddHours(1)
    },
    cancellationToken
);
```

### Message ID, session ID, and application properties

`AddQueue` and `AddTopic` accept a `configureMessage` action that receives
the outgoing `ServiceBusMessage` and the message object. It runs after
serialization, on every message, so call sites don't have to set these
values:

```csharp
using YetAnotherBusWrapper.Sending;

bus.AddTopic<PriceChanged>(
    "prices",
    configureMessage: (serviceBusMessage, message) =>
    {
        serviceBusMessage.MessageId = MessageIdSanitizer.Sanitize(
            $"{message.ProductId}-{message.Price}"
        );
        serviceBusMessage.ApplicationProperties["productId"] =
            message.ProductId;
    }
);
```

When `configureMessage` leaves the message ID empty, the sender sets a new
GUID.

`MessageIdSanitizer.Sanitize` lowercases the value, keeps only letters and
digits, and cuts it to 128 characters. It returns a new GUID when nothing
is left. Use it to build message IDs from content for duplicate detection.

## Consuming

A consumer implements `IMessageConsumer<TMessage>.ConsumeAsync`. It
receives the deserialized message and a `MessageContext` with:

- `MessageId`
- `CorrelationId`
- `DeliveryCount`
- `SessionId`
- `EnqueuedTime`
- `ApplicationProperties`

Consumers are registered as scoped services. Each message is handled in
its own dependency injection scope, so scoped services such as database
contexts are not shared between messages. When `ConsumeAsync` returns
normally, the message is completed and removed from the entity. When it
throws, the message is retried, as described in [Retries and the
dead-letter queue](#retries-and-the-dead-letter-queue).

### Processing options

Queue consumer options and subscription options both have these processing
options:

- `MaxConcurrentCalls` (default `1`). Not used with sessions.
- `PrefetchCount` (default `0`).
- `MaxAutoLockRenewalDuration` (default 5 minutes). `TimeSpan.Zero` turns
  lock renewal off.
- `SessionsEnabled` (default `false`).
- `MaxConcurrentSessions` (default `8`). Used only with sessions.
- `SessionIdleTimeout` (default unset, which uses the Azure SDK default).
  Used only with sessions.

### Sessions

Set `SessionsEnabled` to handle the messages of one session in order. The
queue or subscription must require sessions, and every message needs a
session ID. Set the session ID in `configureMessage`. When the library
creates the queue or subscription, it sets `RequiresSession` from
`SessionsEnabled`, before it runs the create-options callback. An entity
that already exists is left unchanged, so it must already require sessions.

```csharp
bus.AddQueue<OrderPlaced>(
        "orders",
        configureMessage: (serviceBusMessage, message) =>
            serviceBusMessage.SessionId = message.OrderId
    )
    .Configure(options => options.ConnectionString = connectionString)
    .Bind(serviceBus.GetSection("Orders"));
bus.AddQueueConsumer<OrderPlacedConsumer, OrderPlaced>()
    .Configure(options =>
    {
        options.SessionsEnabled = true;
        options.MaxConcurrentSessions = 4;
        options.SessionIdleTimeout = TimeSpan.FromSeconds(30);
    });
```

### Correlation ID and logging

Every received message gets a correlation ID: the message's own
`CorrelationId` when it is set, and a new GUID otherwise. The library
opens a logging scope with the correlation ID, message ID, logical entity
name, and delivery count, so every log written while the message is
handled can be found together. Consumer exceptions are logged once, in
that scope. A consumer cancelled while the processor stops is logged at
`Information` instead of `Error`. Other errors reported by the Service Bus
processor, such as connection problems, are logged with the logical entity
name.

## Retries and the dead-letter queue

There is one failure rule:

1. When a consumer throws, the library logs the exception, and the message
   goes back to the entity without being completed.
2. Service Bus delivers the message again, and `MessageContext.DeliveryCount`
   increases by one.
3. When the delivery count reaches the entity's max delivery count, Service
   Bus moves the message to the dead-letter queue with the reason
   `MaxDeliveryCountExceeded`.

A message that can't be deserialized follows the same rule. The library
has no other failure handling: no exception hooks, failure classification,
or explicit dead-lettering. To give up on a message sooner, lower the max
delivery count. To recover from a longer outage, raise it.

The max delivery count decides how many times a failing message is
retried, so choose it for each entity. `MaxDeliveryCount` on queue and
subscription options defaults to `10`, and it is applied only when the
library creates the entity. For an entity that already exists, the value
set on the entity in Service Bus applies. With `MaxDeliveryCount` set to
`1`, the first failure moves the message to the dead-letter queue.

Retries happen immediately, so a consumer that fails because a dependency
is down can use up all deliveries within seconds. Consumers should also be
idempotent, because a message is delivered again when the consumer
succeeds but completing the message fails.

## Provisioning

When the host starts, the library creates entities whose options set
`CreateOnStartup`. Queues and topics are created first, then subscriptions.
Consumers start after provisioning. An entity that already exists is left
unchanged and counts as success, so restarts are safe.

The library builds the create options from `MaxDeliveryCount` and
`DuplicateDetectionWindow`. A window greater than zero turns on duplicate
detection, so Service Bus drops repeated messages with the same message ID
within the window. For any other property, pass a callback that changes the
create options before the entity is created:

```csharp
bus.AddQueue<OrderPlaced>(
    "orders",
    configureCreateOptions: options =>
    {
        options.LockDuration = TimeSpan.FromMinutes(1);
        options.DefaultMessageTimeToLive = TimeSpan.FromDays(7);
    }
);
bus.AddTopic<PriceChanged>(
    "prices",
    configureCreateOptions: options => options.MaxSizeInMegabytes = 2048
);
bus.AddTopicConsumer<PriceCacheConsumer, PriceChanged>(
    "cache",
    configureCreateOptions: options =>
        options.DeadLetteringOnMessageExpiration = true
);
```

By default, a provisioning failure is logged and startup continues, so an
application with only send and listen rights still runs. Set
`FailStartupOnProvisioningError` on an entity to make its provisioning
failure stop startup.

When the host stops, all consumers stop in parallel first. Then the library
deletes the entities whose options set `DeleteOnShutdown`, subscriptions
first. Deletion also runs when a consumer fails to stop or the host shutdown
timeout elapses, and it does not observe the shutdown token. Deletion
failures are logged and never stop shutdown.

If startup fails after provisioning has begun, the library stops the
consumers it started and deletes the `DeleteOnShutdown` entities before it
rethrows the startup error. If this rollback fails, the library logs the
rollback failure and still rethrows the startup error.

## Entity names and the instance suffix

The entity name in the namespace is built from the configured name
(`QueueName`, `TopicName`, or `SubscriptionName`):

1. Every character except ASCII letters, digits, and hyphens is removed
   from the configured name and from the instance suffix, and leading and
   trailing hyphens are trimmed, and the rest is lowercased. A configured
   name or instance suffix with no letters or digits left fails with
   `InvalidOperationException`. When this step removes characters from the
   configured name, the library logs a warning with the configured and the
   changed name. Lowercasing alone logs nothing.
2. When `UseInstanceSuffix` is set, `-` and the instance suffix are added.
3. The name is cut to 260 characters for queues and topics and 50 for
   subscriptions. Only the configured part is cut, so the instance suffix
   stays whole. An instance suffix longer than 16 characters is replaced
   with the first 16 hex characters of its SHA-256 hash. Hyphens left at the
   end of the cut part are trimmed. Cutting logs a warning with the full and
   the cut name.

The instance suffix gives each running instance of an application its own
entity. By default it is the lowercased, trimmed `WEBSITE_INSTANCE_ID`
environment variable when App Service sets it, and the lowercased machine
name otherwise. Replace it with your own `IInstanceNameProvider`:

```csharp
using YetAnotherBusWrapper.Naming;

public sealed class PodNameProvider : IInstanceNameProvider
{
    public string GetInstanceName()
    {
        return Environment.GetEnvironmentVariable("POD_NAME") ?? "local";
    }
}

builder.Services.AddYetAnotherBusWrapper(bus =>
{
    bus.UseInstanceNameProvider<PodNameProvider>();
});
```

`UseInstanceNameProvider` also has an overload that takes an instance.

## Disabled entities

Set `Enabled` to `false` to deploy an application to an environment where
an entity doesn't exist. A disabled entity:

- is not provisioned or deleted,
- has no consumer running,
- skips options validation, so its connection string can be empty,
- reports `Degraded` from its health check.

A subscription counts as disabled when its topic is disabled, so setting
`Enabled` to `false` on a topic also disables all of its subscriptions.

Sending to a disabled entity throws `InvalidOperationException` that names
the logical entity name, unless the sender is best-effort.

## Best-effort senders

Set `BestEffort` on queue or topic options to make its sender best-effort.
Use it for optional messages, such as notifications, that must not break
the main operation. Best-effort senders implement the same
`IMessageSender<TMessage>`, so switching doesn't change core code.

- Sending to a disabled entity logs it and returns `SendStatus.Skipped`.
- A send failure is logged and returns `SendStatus.Failed` with the IDs of
  the messages already sent. Cancellation still throws
  `OperationCanceledException`.

```csharp
SendResult result = await _sender.SendAsync(
    message,
    cancellationToken: cancellationToken
);
if (result.Status != SendStatus.Sent)
{
    _logger.LogInformation("Notification not sent: {Status}", result.Status);
}
```

## Clients and lifecycle

All registrations that use the same connection string share one
`ServiceBusClient`, so an application opens one connection per namespace.
The same applies to the administration client, which is shared per
`AdministrationConnectionString`, or per `ConnectionString` when
`AdministrationConnectionString` is not set.

Both clients use the `ClientRetry` options of the registrations. The
administration client applies `MaxRetries`, `MaxDelay`, and `TryTimeout` to
each HTTP request it sends for provisioning and existence health checks. If
two enabled queues or topics use the same connection string or the same
administration connection string with different `ClientRetry` options,
startup fails with a message that names both logical entity names. With
hosted services turned off, the same error is thrown when the first client
is created.

The library adds one hosted service. It provisions entities and starts
consumers when the host starts, and stops consumers and deletes entities
when the host stops. Starting and stopping more than once is safe.
Processors and clients are disposed when the host is disposed.
A service provider built with `BuildServiceProvider()` can also be disposed
synchronously, but `DisposeAsync()` is preferred because synchronous disposal
blocks while the Service Bus connections close.

Tools that build the service container without running the application,
such as OpenAPI document generation, can turn off the hosted service.
Nothing is provisioned and no consumer starts:

```csharp
builder.Services.AddYetAnotherBusWrapper(bus =>
{
    bus.DisableHostedServices();
});
```

## Health checks

Add one health check per registered queue, topic, and subscription:

```csharp
builder.Services.AddHealthChecks().AddYetAnotherBusWrapper();

WebApplication app = builder.Build();
app.MapHealthChecks("/health");
```

Checks are named `yetanotherbuswrapper-<kind>-<logical entity name>`, for
example `yetanotherbuswrapper-queue-orders` and
`yetanotherbuswrapper-subscription-prices/cache`.

- `HealthCheckKind.Existence` (default) checks that the entity exists,
  using the administration client. It needs manage rights.
- `HealthCheckKind.Peek` peeks one message from a queue or subscription.
  It works with listen rights.
- A disabled entity reports `Degraded`.
- A missing entity reports `Unhealthy` with a description that names the
  entity.
- A check that throws reports `Unhealthy` with the exception message.

## Per-instance broadcast subscriptions

To invalidate an in-memory cache on every running instance, each instance
needs its own subscription on a shared topic. The library has no dedicated
API for this. Combine these options on the subscription:

- `UseInstanceSuffix` gives each instance its own subscription name.
- `AutoDeleteOnIdle`, set through the create-options callback, removes
  subscriptions of instances that stopped without a clean shutdown. Service
  Bus requires at least 5 minutes.
- `CreateOnStartup` creates the subscription when the instance starts.
- `DeleteOnShutdown` deletes it when the instance stops cleanly.

When several applications share the topic, put the application name in the
message, and let each consumer ignore messages meant for other
applications:

```csharp
public sealed record CacheInvalidated(string ApplicationName, string Key);
```

```csharp
bus.AddTopic<CacheInvalidated>("cache-invalidation")
    .Configure(options =>
    {
        options.ConnectionString = connectionString;
        options.TopicName = "cache-invalidation";
    });
bus.AddTopicConsumer<CacheInvalidatedConsumer, CacheInvalidated>(
        "local-cache",
        configureCreateOptions: options =>
            options.AutoDeleteOnIdle = TimeSpan.FromHours(1)
    )
    .Configure(options =>
    {
        options.SubscriptionName = "app-name";
        options.UseInstanceSuffix = true;
        options.CreateOnStartup = true;
        options.DeleteOnShutdown = true;
    });
```

```csharp
using Microsoft.Extensions.Caching.Memory;
using YetAnotherBusWrapper.Abstractions;

public sealed class CacheInvalidatedConsumer
    : IMessageConsumer<CacheInvalidated>
{
    private const string ApplicationName = "app-name";

    private readonly IMemoryCache _cache;

    public CacheInvalidatedConsumer(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task ConsumeAsync(
        CacheInvalidated message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        if (message.ApplicationName != ApplicationName)
        {
            return Task.CompletedTask;
        }

        _cache.Remove(message.Key);

        return Task.CompletedTask;
    }
}
```

`WEBSITE_INSTANCE_ID` is 64 characters long, and subscription names are
cut to 50 characters. When the name doesn't fit, the instance ID is replaced
with a 16-character hash and `SubscriptionName` is cut to 33 characters, so
each instance still gets its own subscription. Keep `SubscriptionName` short,
or replace the instance name provider with a shorter value, to keep the
names readable.

## Keeping core code independent of Service Bus

Application core code can reference only `YetAnotherBusWrapper.Abstractions`.
It depends on `IMessageSender<TMessage>` and implements
`IMessageConsumer<TMessage>`, and it doesn't see Azure SDK types. The host
project references `YetAnotherBusWrapper` and registers the entities.

When core code shouldn't know about messaging at all, define a port in the
core project and implement it in the host project with
`IMessageSender<TMessage>`:

```csharp
public interface IOrderNotifier
{
    Task NotifyOrderPlacedAsync(
        string orderId,
        decimal total,
        CancellationToken cancellationToken
    );
}
```

```csharp
using YetAnotherBusWrapper.Abstractions;

public sealed class ServiceBusOrderNotifier : IOrderNotifier
{
    private readonly IMessageSender<OrderPlaced> _sender;

    public ServiceBusOrderNotifier(IMessageSender<OrderPlaced> sender)
    {
        _sender = sender;
    }

    public async Task NotifyOrderPlacedAsync(
        string orderId,
        decimal total,
        CancellationToken cancellationToken
    )
    {
        await _sender.SendAsync(
            new OrderPlaced(orderId, total),
            cancellationToken: cancellationToken
        );
    }
}
```

```csharp
builder.Services.AddSingleton<IOrderNotifier, ServiceBusOrderNotifier>();
```

Senders are singletons, so the port implementation can be a singleton too.

## Sample application

`samples/` contains an ASP.NET Core application that uses every feature
described here against a real Service Bus namespace, with one HTTP
endpoint per scenario. See [samples/README.md](samples/README.md).
