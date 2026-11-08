using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace YetAnotherBusWrapper.Sample.Api.HealthChecks;

internal static class HealthReportJsonWriter
{
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        HealthReportResponse response = new(
            report.Status,
            report.TotalDuration,
            [
                .. report.Entries.Select(
                    entry => new HealthEntryResponse(
                        entry.Key,
                        entry.Value.Status,
                        entry.Value.Description,
                        entry.Value.Duration
                    )
                )
            ]
        );

        return context.Response.WriteAsJsonAsync(response, context.RequestAborted);
    }
}
