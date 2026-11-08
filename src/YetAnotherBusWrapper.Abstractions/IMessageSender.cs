namespace YetAnotherBusWrapper.Abstractions;

public interface IMessageSender<in TMessage>
{
    Task<SendResult> SendAsync(
        TMessage message,
        SendOptions? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Sends the messages in as many batches as needed. Each batch is filled until it is full, sent, and the
    ///     remaining messages continue in a new batch.
    /// </summary>
    /// <remarks>
    ///     A send split into several batches is not atomic. If a later batch fails, or a message doesn't fit into an
    ///     empty batch, the batches sent before it stay sent.
    /// </remarks>
    Task<SendResult> SendAsync(
        IReadOnlyCollection<TMessage> messages,
        SendOptions? options = null,
        CancellationToken cancellationToken = default
    );
}
