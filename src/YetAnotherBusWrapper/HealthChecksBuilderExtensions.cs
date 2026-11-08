using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.HealthChecks;

namespace YetAnotherBusWrapper;

public static class HealthChecksBuilderExtensions
{
    public static IHealthChecksBuilder AddYetAnotherBusWrapper(this IHealthChecksBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddSingleton<EntityHealthChecker>();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigureOptions<HealthCheckServiceOptions>, EntityHealthCheckRegistrations>()
        );

        return builder;
    }
}
