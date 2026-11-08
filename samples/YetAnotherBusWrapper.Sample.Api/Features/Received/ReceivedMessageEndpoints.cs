using Microsoft.AspNetCore.Http.HttpResults;

namespace YetAnotherBusWrapper.Sample.Api.Features.Received;

internal static class ReceivedMessageEndpoints
{
    public static void MapReceivedMessageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/received", GetReceivedMessages);
    }

    private static Ok<IReadOnlyList<ReceivedMessage>> GetReceivedMessages(ReceivedMessageLog receivedMessageLog)
    {
        return TypedResults.Ok(receivedMessageLog.GetAll());
    }
}
