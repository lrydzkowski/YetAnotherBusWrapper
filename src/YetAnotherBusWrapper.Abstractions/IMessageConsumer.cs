namespace YetAnotherBusWrapper.Abstractions;

public interface IMessageConsumer<in TMessage>
{
    Task ConsumeAsync(TMessage message, MessageContext context, CancellationToken cancellationToken);
}
