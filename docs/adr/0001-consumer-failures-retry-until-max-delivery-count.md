# Every consumer failure is retried until the max delivery count

When a consumer throws, the message is abandoned and Service Bus delivers it
again. After `MaxDeliveryCount` deliveries, Service Bus moves the message to
the dead-letter queue with reason `MaxDeliveryCountExceeded`. The library
relies on `AutoCompleteMessages = true` in `ServiceBusProcessor` and never
settles messages itself. Consumers have no exception hook, no failure
classification, and no way to dead-letter explicitly.

## Consequences

- Permanent failures, including messages that cannot be deserialized, are
  retried like transient ones until they reach the dead-letter queue.
- `MaxDeliveryCount` for provisioned queues and subscriptions is a plain
  option, with a default of 10 (the Service Bus default).
- The library logs each consumer exception inside the message's logging scope
  and rethrows it.
- A consumer that wants a failure treated as success must catch the exception
  itself.
