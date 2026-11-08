using Microsoft.AspNetCore.Http.HttpResults;
using YetAnotherBusWrapper.Abstractions;

namespace YetAnotherBusWrapper.Sample.Api.Features.Payments;

internal static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/payments", RequestPaymentAsync);
    }

    private static async Task<Ok<SendResult>> RequestPaymentAsync(
        RequestPaymentRequest request,
        IMessageSender<PaymentRequested> sender,
        CancellationToken cancellationToken
    )
    {
        PaymentRequested message = new(Guid.NewGuid().ToString(), request.Amount, request.FailuresToSimulate);
        SendResult result = await sender.SendAsync(message, cancellationToken: cancellationToken);

        return TypedResults.Ok(result);
    }
}

internal sealed record RequestPaymentRequest(decimal Amount, int FailuresToSimulate);
