using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Clients;
using YetAnotherBusWrapper.Consuming;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Provisioning;
using YetAnotherBusWrapper.Registration;
using YetAnotherBusWrapper.Sending;
using YetAnotherBusWrapper.Serialization;

namespace YetAnotherBusWrapper;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddYetAnotherBusWrapper(
        this IServiceCollection services,
        Action<YetAnotherBusWrapperBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddCoreServices();
        YetAnotherBusWrapperRegistry registry = services.GetOrAddRegistry();
        configure(new YetAnotherBusWrapperBuilder(services, registry));
        services.ConfigureHostedService(registry);

        return services;
    }

    private static void AddCoreServices(this IServiceCollection services)
    {
        services.AddOptions();
        services.TryAddSingleton<ServiceBusClientProvider>();
        services.TryAddSingleton<ClientRetryOptionsValidator>();
        services.TryAddSingleton<IMessageSerializer, JsonMessageSerializer>();
        services.TryAddSingleton<IInstanceNameProvider, EnvironmentInstanceNameProvider>();
        services.TryAddSingleton<EntityNameResolver>();
        services.TryAddSingleton<ServiceBusSenderFactory>();
        services.TryAddSingleton<ServiceBusProcessorFactory>();
        services.TryAddSingleton<QueueProvisioner>();
        services.TryAddSingleton<TopicProvisioner>();
        services.TryAddSingleton<SubscriptionProvisioner>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<QueueOptions>, QueueOptionsValidator>()
        );
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TopicOptions>, TopicOptionsValidator>()
        );
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<SubscriptionOptions>, SubscriptionOptionsValidator>()
        );
    }

    private static void ConfigureHostedService(this IServiceCollection services, YetAnotherBusWrapperRegistry registry)
    {
        if (!registry.HostedServicesDisabled)
        {
            services.AddHostedService<YetAnotherBusWrapperHostedService>();

            return;
        }

        ServiceDescriptor? descriptor = services.FirstOrDefault(
            descriptor => descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType == typeof(YetAnotherBusWrapperHostedService)
        );
        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }
    }

    private static YetAnotherBusWrapperRegistry GetOrAddRegistry(this IServiceCollection services)
    {
        ServiceDescriptor? descriptor = services.FirstOrDefault(
            descriptor => descriptor.ServiceType == typeof(YetAnotherBusWrapperRegistry)
        );
        if (descriptor?.ImplementationInstance is YetAnotherBusWrapperRegistry existingRegistry)
        {
            return existingRegistry;
        }

        YetAnotherBusWrapperRegistry registry = new();
        services.AddSingleton(registry);

        return registry;
    }
}
