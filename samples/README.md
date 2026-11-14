# YetAnotherBusWrapper sample application

An ASP.NET Core application that uses every feature of YetAnotherBusWrapper
against a real Azure Service Bus namespace. Each scenario has an HTTP
endpoint that sends a message, and a consumer that records what it
received.

## Project

`YetAnotherBusWrapper.Sample.Api` references `YetAnotherBusWrapper`. It
holds the messages, the consumers, `OrderService` with its `IOrderNotifier`
port, and the in-memory `ReceivedMessageLog` and `PriceCache`. It registers
all entities in `ServiceBus/ServiceBusServiceCollectionExtensions.cs`, maps
the endpoints, and implements `IOrderNotifier` with
`IMessageSender<OrderPlaced>`.

## Prerequisites

- A Service Bus namespace on the Standard or Premium tier. The Basic tier
  has no topics, and the sample also uses sessions and duplicate
  detection.
- A connection string with Manage rights. The application creates its
  queues, topics, and subscriptions on startup, deletes the per-instance
  subscription on shutdown, and runs existence health checks. All of these
  need Manage rights.

To run with a Send and Listen connection string, create the entities
yourself, set `CreateOnStartup`, `DeleteOnShutdown`, and
`FailStartupOnProvisioningError` to `false`, and set `HealthCheck` to
`Peek` on queues and subscriptions. Topic health checks always check
existence, so they report `Unhealthy` without Manage rights. Alternatively,
keep a Send and Listen connection string in `ServiceBus:ConnectionString`
and put a Manage connection string in
`ServiceBus:AdministrationConnectionString`, which provisioning and
existence checks use instead.

## Run

Store the connection string in user secrets and start the application:

```shell
cd samples/YetAnotherBusWrapper.Sample.Api
dotnet user-secrets set "ServiceBus:ConnectionString" "<connection string>"
dotnet run
```

The application listens on `http://localhost:5180`. Without a connection
string, startup fails with one validation error per enabled queue and
topic, for example `Queue 'orders' requires ConnectionString.`

Send the requests in `YetAnotherBusWrapper.Sample.Api.http` from Visual
Studio, Rider, or the VS Code REST Client extension. Then check:

- the console log, where each consumer log line has the library's logging
  scope with the correlation ID, message ID, logical entity name, and
  delivery count,
- `GET /received`, which lists the messages that consumers handled, with
  their `MessageContext` values,
- `GET /health`, which lists one health check per queue, topic, and
  subscription.

## Configuration

All values come from the `ServiceBus` and `Sample` sections of
`appsettings.json`. The library doesn't read configuration itself. Each
registration binds the whole `ServiceBus` section first, which provides
the shared `ConnectionString`, `AdministrationConnectionString`, and
`ClientRetry`. It then binds its own subsection, which can override them.

`Sample` settings:

- `InstanceName`: the instance suffix for entities that set
  `UseInstanceSuffix`. When empty, the machine name is used. Registered
  with `UseInstanceNameProvider`.
- `UseCustomMessageSerializer`: replaces the JSON serializer with
  `MessageTypeJsonSerializer`, which writes the same JSON and adds a
  `messageType` application property to every message. Registered with
  `UseMessageSerializer`.
- `DisableHostedServices`: calls `DisableHostedServices`. Nothing is
  provisioned and no consumer starts, but senders and health checks work.
  Use it for a send-only instance or for tools that only build the
  service container.

Any setting can also come from an environment variable, for example
`Sample__InstanceName=instance-2`.

## Scenarios

### Orders

`POST /orders` sends to the `orders` queue.

- `OrderService` sends through the `IOrderNotifier` port and doesn't know
  about Service Bus.
- `configureMessage` sets the correlation ID to the order ID.
- The queue is created with `MaxDeliveryCount` 5, and through
  `configureCreateOptions`, with a 1-minute lock duration and a 7-day time
  to live.
- `FailStartupOnProvisioningError` stops startup if the queue can't be
  created.
- The consumer runs with `MaxConcurrentCalls` 4, `PrefetchCount` 8, and a
  10-minute `MaxAutoLockRenewalDuration`.

### Retries and the dead-letter queue

`POST /payments` sends to the `payments` queue, which has
`MaxDeliveryCount` 3. The consumer throws until `DeliveryCount` is greater
than `failuresToSimulate`, and the library logs each failure.

- With `failuresToSimulate` 1, the message succeeds on the second
  delivery.
- With `failuresToSimulate` 5, every delivery fails. After the third,
  Service Bus moves the message to the dead-letter queue with the reason
  `MaxDeliveryCountExceeded`. Look for it in the Service Bus Explorer in
  the Azure portal.

### Sessions

`POST /shipments/{id}/statuses` sends a list of statuses to the
`shipments` queue.

- `configureMessage` sets the session ID to the shipment ID.
- The consumer sets `SessionsEnabled`, `MaxConcurrentSessions`, and
  `SessionIdleTimeout`, so the statuses of one shipment are handled in
  order. Because `SessionsEnabled` is set, the library creates the queue
  with `RequiresSession`.
- The queue's health check uses `Peek`.

### Collections and scheduled enqueue

`POST /reports` sends `count` messages to the `reports` queue in one call.
The sender splits them into as many batches as needed. A positive
`delaySeconds` sets `SendOptions.ScheduledEnqueueTime`, and the consumer
receives the messages when they are due.

### Disabled entities

`POST /audit` sends to the `audit` queue, which has `Enabled` set to
`false`. No consumer runs, nothing is provisioned, and the health check
reports `Degraded`. Sending throws `InvalidOperationException`, which the
endpoint returns as 409.

The `SAMPLE_AUDIT_QUEUE_NAME` environment variable overrides the queue
name. Host code reads it in `PostConfigure`, which shows how to fill
options from environment variables.

### Topics, message IDs, and duplicate detection

`POST /prices` sends to the `prices` topic. `GET /prices/{productId}`
reads the price that the `cache` consumer stored.

- The `cache` and `search` subscriptions each have a consumer, and both
  receive every message.
- `configureMessage` builds the message ID from the content with
  `MessageIdSanitizer` and adds a `productId` application property.
- The topic has a 10-minute `DuplicateDetectionWindow`, so the same price
  sent twice is delivered once.
- The `cache` subscription is created with
  `DeadLetteringOnMessageExpiration`.

### Per-instance broadcast subscriptions

`POST /cache-invalidations` sends to the `cache-invalidation` topic, and
every running instance removes the price from its own cache.

- `UseInstanceSuffix` gives each instance its own `local-cache`
  subscription.
- `AutoDeleteOnIdle` (1 hour) removes subscriptions of instances that
  stopped without a clean shutdown.
- `CreateOnStartup` and `DeleteOnShutdown` create and delete the
  subscription with the instance.
- The consumer ignores messages whose `applicationName` belongs to another
  application.

To see two instances receive the message, start a second one on another
port with another instance name:

```shell
dotnet run -- --urls http://localhost:5181 --Sample:InstanceName=instance-2
```

### Best-effort senders

`POST /notifications` sends to the `notifications` topic, which has
`BestEffort` set to `true`. The endpoint returns `Sent`. Set
`ServiceBus:Notifications:Enabled` to `false`, and the same request
returns `Skipped` instead of throwing. A send failure returns `Failed`.

### Health checks

`GET /health` returns the result of
`AddHealthChecks().AddYetAnotherBusWrapper()`, with one check per entity
named `yetanotherbuswrapper-<kind>-<logical entity name>`.

### Client sharing

All queues and topics use the same connection string and `ClientRetry`
options, so they share one `ServiceBusClient`.

## Errors that stop the application

These rules fail at registration or startup, so the sample doesn't
contain them. To see one, add it to `ServiceBusServiceCollectionExtensions.cs`:

- A second `AddQueue` or `AddTopic` for a message type that is already
  registered.
- A second `AddQueueConsumer` for the same queue.
- `AddQueueConsumer` for a message type registered with `AddTopic`, or
  `AddTopicConsumer` for one registered with `AddQueue`.
- The same subscription name passed to `AddTopicConsumer` twice for one
  topic.
- Two enabled queues or topics that use the same connection string or the
  same administration connection string with different `ClientRetry`
  options.
