using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace YetAnotherBusWrapper.HealthChecks;

internal sealed class EntityHealthCheck : IHealthCheck
{
    private readonly Func<CancellationToken, Task<HealthCheckResult>> _check;

    public EntityHealthCheck(Func<CancellationToken, Task<HealthCheckResult>> check)
    {
        _check = check;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        return _check(cancellationToken);
    }
}
