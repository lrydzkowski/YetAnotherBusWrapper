using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace YetAnotherBusWrapper.Sample.Api.HealthChecks;

internal sealed record HealthEntryResponse(
    string Name,
    HealthStatus Status,
    string? Description,
    TimeSpan Duration
);
