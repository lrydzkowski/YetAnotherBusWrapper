using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Serialization;

namespace YetAnotherBusWrapper.Sample.Api.ServiceBus;

internal sealed class MessageTypeJsonSerializer : IMessageSerializer
{
    private const string JsonContentType = "application/json";
    private const string MessageTypePropertyName = "messageType";

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public MessageTypeJsonSerializer(IOptions<MessageSerializerOptions> options)
    {
        _jsonSerializerOptions = options.Value.JsonSerializerOptions;
    }

    public ServiceBusMessage Serialize<TMessage>(TMessage message)
    {
        return new ServiceBusMessage(BinaryData.FromObjectAsJson(message, _jsonSerializerOptions))
        {
            ContentType = JsonContentType,
            ApplicationProperties =
            {
                [MessageTypePropertyName] = typeof(TMessage).Name
            }
        };
    }

    public TMessage Deserialize<TMessage>(ServiceBusReceivedMessage message)
    {
        TMessage? deserializedMessage = message.Body.ToObjectFromJson<TMessage>(_jsonSerializerOptions);
        if (deserializedMessage is null)
        {
            throw new InvalidOperationException(
                $"Message '{message.MessageId}' body deserialized to null for message type '{typeof(TMessage).Name}'."
            );
        }

        return deserializedMessage;
    }
}
