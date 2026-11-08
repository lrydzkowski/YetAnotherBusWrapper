using System.Text.Json;
using System.Text.Json.Serialization;

namespace YetAnotherBusWrapper.Options;

public sealed class MessageSerializerOptions
{
    public JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
}
