using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Serialization;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sending.Data;

internal sealed class PlainTextMessageSerializer : IMessageSerializer
{
    public ServiceBusMessage Serialize<TMessage>(TMessage message)
    {
        return new ServiceBusMessage(message?.ToString()) { ContentType = "text/plain" };
    }

    public TMessage Deserialize<TMessage>(ServiceBusReceivedMessage message)
    {
        throw new NotSupportedException("Plain text messages are only sent in these tests.");
    }
}
