using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.HealthChecks;

internal sealed class EntityHealthCheckRegistrations : IConfigureOptions<HealthCheckServiceOptions>
{
    private const string NamePrefix = "yetanotherbuswrapper";

    private readonly IServiceProvider _serviceProvider;

    public EntityHealthCheckRegistrations(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Configure(HealthCheckServiceOptions options)
    {
        YetAnotherBusWrapperRegistry registry = _serviceProvider.GetService<YetAnotherBusWrapperRegistry>()
            ?? throw new InvalidOperationException(
                "YetAnotherBusWrapper health checks require AddYetAnotherBusWrapper to be called on the service collection."
            );

        foreach (QueueRegistration queue in registry.Queues)
        {
            options.Registrations.Add(
                CreateRegistration(
                    EntityKind.Queue,
                    queue.LogicalEntityName,
                    (checker, cancellationToken) => checker.CheckQueueAsync(queue.LogicalEntityName, cancellationToken)
                )
            );
        }

        foreach (TopicRegistration topic in registry.Topics)
        {
            options.Registrations.Add(
                CreateRegistration(
                    EntityKind.Topic,
                    topic.LogicalEntityName,
                    (checker, cancellationToken) => checker.CheckTopicAsync(topic.LogicalEntityName, cancellationToken)
                )
            );
        }

        foreach (SubscriptionRegistration subscription in registry.Subscriptions)
        {
            options.Registrations.Add(
                CreateRegistration(
                    EntityKind.Subscription,
                    subscription.LogicalEntityName,
                    (checker, cancellationToken) => checker.CheckSubscriptionAsync(subscription, cancellationToken)
                )
            );
        }
    }

    private static HealthCheckRegistration CreateRegistration(
        EntityKind kind,
        string logicalEntityName,
        Func<EntityHealthChecker, CancellationToken, Task<HealthCheckResult>> check
    )
    {
        return new HealthCheckRegistration(
            $"{NamePrefix}-{kind.ToLogName()}-{logicalEntityName}",
            serviceProvider => new EntityHealthCheck(
                cancellationToken => check(serviceProvider.GetRequiredService<EntityHealthChecker>(), cancellationToken)
            ),
            null,
            null
        );
    }
}
