using Microsoft.AspNetCore.Http.HttpResults;
using YetAnotherBusWrapper.Abstractions;

namespace YetAnotherBusWrapper.Sample.Api.Features.Audit;

internal static class AuditEndpoints
{
    public static void MapAuditEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/audit", RecordAuditEntryAsync);
    }

    private static async Task<Results<Ok<SendResult>, ProblemHttpResult>> RecordAuditEntryAsync(
        RecordAuditEntryRequest request,
        IMessageSender<AuditEntryRecorded> sender,
        CancellationToken cancellationToken
    )
    {
        try
        {
            SendResult result = await sender.SendAsync(
                new AuditEntryRecorded(request.Action, request.UserName),
                cancellationToken: cancellationToken
            );

            return TypedResults.Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            return TypedResults.Problem(exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }
}

internal sealed record RecordAuditEntryRequest(string Action, string UserName);
