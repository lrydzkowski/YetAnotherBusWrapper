using Azure.Messaging.ServiceBus;

namespace YetAnotherBusWrapper.Serialization;

public interface IMessageSerializer
{
    ServiceBusMessage Serialize<TMessage>(TMessage message);

    TMessage Deserialize<TMessage>(ServiceBusReceivedMessage message);
}
