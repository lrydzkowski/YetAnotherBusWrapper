using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Serialization;

internal sealed class JsonMessageSerializer : IMessageSerializer
{
    private const string JsonContentType = "application/json";

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JsonMessageSerializer(IOptions<MessageSerializerOptions> options)
    {
        _jsonSerializerOptions = options.Value.JsonSerializerOptions;
    }

    public ServiceBusMessage Serialize<TMessage>(TMessage message)
    {
        return new ServiceBusMessage(BinaryData.FromObjectAsJson(message, _jsonSerializerOptions))
        {
            ContentType = JsonContentType
        };
    }

    public TMessage Deserialize<TMessage>(ServiceBusReceivedMessage message)
    {
        TMessage? deserializedMessage = message.Body.ToObjectFromJson<TMessage>(_jsonSerializerOptions);
        if (deserializedMessage is null)
        {
            throw new InvalidOperationException(
                $"Message '{message.MessageId}' body deserialized to null for message type '{typeof(TMessage).FullName}'."
            );
        }

        return deserializedMessage;
    }
}
