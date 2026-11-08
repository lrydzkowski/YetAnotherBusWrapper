using System.Text.Json;
using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Batching.Data;

internal record QueuedReportDto(string MessageId, int Number, int BodyLength)
{
    public static QueuedReportDto From(PeekedMessageDto message)
    {
        using JsonDocument body = JsonDocument.Parse(message.Body);

        return new QueuedReportDto(
            message.MessageId,
            body.RootElement.GetProperty("number").GetInt32(),
            message.Body.Length
        );
    }
}
