using Microsoft.AspNetCore.Http.HttpResults;
using YetAnotherBusWrapper.Abstractions;

namespace YetAnotherBusWrapper.Sample.Api.Features.Shipments;

internal static class ShipmentEndpoints
{
    public static void MapShipmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/shipments/{shipmentId}/statuses", ChangeStatusesAsync);
    }

    private static async Task<Ok<SendResult>> ChangeStatusesAsync(
        string shipmentId,
        ChangeShipmentStatusesRequest request,
        IMessageSender<ShipmentStatusChanged> sender,
        CancellationToken cancellationToken
    )
    {
        List<ShipmentStatusChanged> messages =
        [
            .. request.Statuses.Select(
                (status, index) => new ShipmentStatusChanged(shipmentId, index + 1, status)
            )
        ];
        SendResult result = await sender.SendAsync(messages, cancellationToken: cancellationToken);

        return TypedResults.Ok(result);
    }
}

internal sealed record ChangeShipmentStatusesRequest(IReadOnlyList<string> Statuses);
