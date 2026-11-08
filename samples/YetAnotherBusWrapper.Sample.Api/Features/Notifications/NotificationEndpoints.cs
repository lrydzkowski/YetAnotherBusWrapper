using Microsoft.AspNetCore.Http.HttpResults;
using YetAnotherBusWrapper.Abstractions;

namespace YetAnotherBusWrapper.Sample.Api.Features.Notifications;

internal static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/notifications", NotifyCustomerAsync);
    }

    private static async Task<Ok<SendResult>> NotifyCustomerAsync(
        NotifyCustomerRequest request,
        IMessageSender<CustomerNotified> sender,
        CancellationToken cancellationToken
    )
    {
        SendResult result = await sender.SendAsync(
            new CustomerNotified(request.CustomerId, request.Text),
            cancellationToken: cancellationToken
        );

        return TypedResults.Ok(result);
    }
}

internal sealed record NotifyCustomerRequest(string CustomerId, string Text);
