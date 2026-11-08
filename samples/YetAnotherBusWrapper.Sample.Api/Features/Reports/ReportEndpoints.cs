using Microsoft.AspNetCore.Http.HttpResults;
using YetAnotherBusWrapper.Abstractions;

namespace YetAnotherBusWrapper.Sample.Api.Features.Reports;

internal static class ReportEndpoints
{
    private const int MaxCount = 1000;

    public static void MapReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/reports", RequestReportsAsync);
    }

    private static async Task<Results<Ok<SendResult>, ValidationProblem>> RequestReportsAsync(
        RequestReportsRequest request,
        IMessageSender<ReportRequested> sender,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        Dictionary<string, string[]> errors = Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        IReadOnlyCollection<ReportRequested> messages = CreateMessages(request);
        SendOptions? options = CreateSendOptions(request, timeProvider);
        SendResult result = await sender.SendAsync(messages, options, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static Dictionary<string, string[]> Validate(RequestReportsRequest request)
    {
        Dictionary<string, string[]> errors = [];
        if (request.Count is < 1 or > MaxCount)
        {
            errors[nameof(request.Count)] = [$"Count must be between 1 and {MaxCount}."];
        }

        if (request.DelaySeconds < 0)
        {
            errors[nameof(request.DelaySeconds)] = ["DelaySeconds must not be negative."];
        }

        return errors;
    }

    private static List<ReportRequested> CreateMessages(RequestReportsRequest request)
    {
        return
        [
            .. Enumerable.Range(1, request.Count)
                .Select(number => new ReportRequested(Guid.NewGuid().ToString(), $"{request.Name} #{number}"))
        ];
    }

    private static SendOptions? CreateSendOptions(RequestReportsRequest request, TimeProvider timeProvider)
    {
        if (request.DelaySeconds == 0)
        {
            return null;
        }

        return new SendOptions
        {
            ScheduledEnqueueTime = timeProvider.GetUtcNow().AddSeconds(request.DelaySeconds)
        };
    }
}

internal sealed record RequestReportsRequest(string Name, int Count, int DelaySeconds);
