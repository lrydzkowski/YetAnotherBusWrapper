# YetAnotherBusWrapper

A library that connects ASP.NET Core applications to Azure Service Bus: it
sends messages, consumes them, and provisions the entities they travel
through.

## Language

### Entities

**Entity**:
A queue, topic, or subscription in a Service Bus namespace.
_Avoid_: resource, channel, endpoint

**Logical entity name**:
The name an application uses to refer to an entity in its own options,
independent of the actual name in the namespace.
_Avoid_: key, alias

**Entity name**:
The actual name of an entity in the namespace, after the instance suffix
and sanitizing are applied.
_Avoid_: path, address

**Instance suffix**:
A value appended to an entity name so that each running instance of an
application gets its own entity.
_Avoid_: machine name suffix

**Disabled entity**:
An entity whose options turn it off: it is not provisioned, nothing
consumes from it, and sending to it fails unless the sender is best-effort.
_Avoid_: inactive, switched-off

**Provisioning**:
Creating entities when an application starts and, optionally, deleting
them when it stops.
_Avoid_: infrastructure management, entity creation

### Messages

**Message type**:
The .NET type of a message body. Each message type belongs to exactly one
queue or topic.
_Avoid_: payload type, data type, TData

**Sender**:
The component that serializes messages of one message type and sends
them to their queue or topic.
_Avoid_: publisher, producer

**Best-effort sender**:
A sender that skips sending to a disabled entity and logs send failures
instead of raising them.
_Avoid_: fire-and-forget sender, safe sender

**Consumer**:
Application code that handles received messages of one message type from
a queue or subscription.
_Avoid_: handler, listener, receiver

**Correlation ID**:
An identifier that ties together the logs produced while one received
message is handled.
_Avoid_: operation ID, trace ID

### Failure handling

**Retry**:
A further delivery of a message after its consumer failed, repeated until the
entity's max delivery count is reached.
_Avoid_: abandon, requeue

**Dead-letter queue**:
The subqueue of a queue or subscription that holds messages whose consumer
failed on every allowed delivery.
_Avoid_: poison queue, error queue
