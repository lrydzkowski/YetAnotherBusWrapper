using Microsoft.AspNetCore.Http.HttpResults;

namespace YetAnotherBusWrapper.Sample.Api.Features.Orders;

internal static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/orders", PlaceOrderAsync);
    }

    private static async Task<Ok<PlaceOrderResponse>> PlaceOrderAsync(
        PlaceOrderRequest request,
        OrderService orderService,
        CancellationToken cancellationToken
    )
    {
        string orderId = await orderService.PlaceAsync(request.Total, cancellationToken);

        return TypedResults.Ok(new PlaceOrderResponse(orderId));
    }
}

internal sealed record PlaceOrderRequest(decimal Total);

internal sealed record PlaceOrderResponse(string OrderId);
