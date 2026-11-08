using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace YetAnotherBusWrapper.Tests.Integration.Features.HealthChecks.Data;

internal record HealthCheckEntryDto(string Name, HealthStatus Status, string? Description, string? ExceptionType)
{
    public static HealthCheckEntryDto From(string name, HealthReportEntry entry)
    {
        return new HealthCheckEntryDto(name, entry.Status, entry.Description, entry.Exception?.GetType().Name);
    }
}
