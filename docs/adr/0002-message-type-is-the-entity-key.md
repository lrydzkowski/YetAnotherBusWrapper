# The message type is the entity key

Each message type maps to exactly one queue or topic. Senders are resolved as
`IMessageSender<TMessage>` and consumers implement
`IMessageConsumer<TMessage>`. A logical entity name keys the named options.
Registering the same message type for a second queue or topic fails at
startup.

## Consequences

- Supporting one message type on several entities later requires keyed
  services or a new sender interface.
