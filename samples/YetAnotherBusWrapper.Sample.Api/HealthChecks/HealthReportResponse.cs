using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace YetAnotherBusWrapper.Sample.Api.HealthChecks;

internal sealed record HealthReportResponse(
    HealthStatus Status,
    TimeSpan TotalDuration,
    IReadOnlyList<HealthEntryResponse> Entries
);
