using System.Text.Json.Serialization;
using YetAnotherBusWrapper.Sample.Api.ServiceBus.Options;
using YetAnotherBusWrapper.Sample.Api.Features.Audit;
using YetAnotherBusWrapper.Sample.Api.Features.Notifications;
using YetAnotherBusWrapper.Sample.Api.Features.Orders;
using YetAnotherBusWrapper.Sample.Api.Features.Payments;
using YetAnotherBusWrapper.Sample.Api.Features.Prices;
using YetAnotherBusWrapper.Sample.Api.Features.Reports;
using YetAnotherBusWrapper.Sample.Api.Features.Shipments;
using YetAnotherBusWrapper.Sending;

namespace YetAnotherBusWrapper.Sample.Api.ServiceBus;

internal static class ServiceBusServiceCollectionExtensions
{
    private const string ServiceBusSectionName = "ServiceBus";

    private const string AuditQueueNameVariable = "SAMPLE_AUDIT_QUEUE_NAME";

    public static IServiceCollection AddSampleServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddOptions(configuration).AddServices().AddBus(configuration);
    }

    private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SampleOptions>().Bind(configuration.GetSection(SampleOptions.Position));

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services.AddSingleton<IOrderNotifier, ServiceBusOrderNotifier>();
    }

    private static IServiceCollection AddBus(this IServiceCollection services, IConfiguration configuration)
    {
        SampleOptions sampleOptions = configuration.GetSection(SampleOptions.Position).Get<SampleOptions>()
            ?? new SampleOptions();
        IConfigurationSection serviceBus = configuration.GetSection(ServiceBusSectionName);

        return services.AddYetAnotherBusWrapper(
            busBuilder =>
            {
                busBuilder.ConfigureSerialization(sampleOptions);
                busBuilder.ConfigureInstanceName();
                busBuilder.ConfigureHostedServices(sampleOptions);
                busBuilder.AddOrders(serviceBus);
                busBuilder.AddPayments(serviceBus);
                busBuilder.AddShipments(serviceBus);
                busBuilder.AddReports(serviceBus);
                busBuilder.AddAudit(serviceBus);
                busBuilder.AddPrices(serviceBus);
                busBuilder.AddCacheInvalidation(serviceBus);
                busBuilder.AddNotifications(serviceBus);
            }
        );
    }

    private static void ConfigureSerialization(this YetAnotherBusWrapperBuilder busBuilder, SampleOptions sampleOptions)
    {
        busBuilder.ConfigureJsonSerializerOptions(
            options => options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        );
        if (sampleOptions.UseCustomMessageSerializer)
        {
            busBuilder.UseMessageSerializer<MessageTypeJsonSerializer>();
        }
    }

    private static void ConfigureInstanceName(this YetAnotherBusWrapperBuilder busBuilder)
    {
        busBuilder.UseInstanceNameProvider<ConfiguredInstanceNameProvider>();
    }

    private static void ConfigureHostedServices(this YetAnotherBusWrapperBuilder busBuilder, SampleOptions sampleOptions)
    {
        if (sampleOptions.DisableHostedServices)
        {
            busBuilder.DisableHostedServices();
        }
    }

    private static void AddOrders(this YetAnotherBusWrapperBuilder busBuilder, IConfigurationSection serviceBus)
    {
        busBuilder.AddQueue<OrderPlaced>(
                LogicalEntityNames.Queues.Orders,
                options =>
                {
                    options.LockDuration = TimeSpan.FromMinutes(1);
                    options.DefaultMessageTimeToLive = TimeSpan.FromDays(7);
                },
                (serviceBusMessage, message) => serviceBusMessage.CorrelationId = message.OrderId
            )
            .Bind(serviceBus)
            .Bind(serviceBus.GetSection("Orders"));
        busBuilder.AddQueueConsumer<OrderPlacedConsumer, OrderPlaced>()
            .Bind(serviceBus.GetSection("Orders:Consumer"));
    }

    private static void AddPayments(this YetAnotherBusWrapperBuilder busBuilder, IConfigurationSection serviceBus)
    {
        busBuilder.AddQueue<PaymentRequested>(LogicalEntityNames.Queues.Payments)
            .Bind(serviceBus)
            .Bind(serviceBus.GetSection("Payments"));
        busBuilder.AddQueueConsumer<PaymentRequestedConsumer, PaymentRequested>()
            .Bind(serviceBus.GetSection("Payments:Consumer"));
    }

    private static void AddShipments(this YetAnotherBusWrapperBuilder busBuilder, IConfigurationSection serviceBus)
    {
        busBuilder.AddQueue<ShipmentStatusChanged>(
                LogicalEntityNames.Queues.Shipments,
                configureMessage: (serviceBusMessage, message) => serviceBusMessage.SessionId = message.ShipmentId
            )
            .Bind(serviceBus)
            .Bind(serviceBus.GetSection("Shipments"));
        busBuilder.AddQueueConsumer<ShipmentStatusChangedConsumer, ShipmentStatusChanged>()
            .Bind(serviceBus.GetSection("Shipments:Consumer"));
    }

    private static void AddReports(this YetAnotherBusWrapperBuilder busBuilder, IConfigurationSection serviceBus)
    {
        busBuilder.AddQueue<ReportRequested>(LogicalEntityNames.Queues.Reports)
            .Bind(serviceBus)
            .Bind(serviceBus.GetSection("Reports"));
        busBuilder.AddQueueConsumer<ReportRequestedConsumer, ReportRequested>()
            .Bind(serviceBus.GetSection("Reports:Consumer"));
    }

    private static void AddAudit(this YetAnotherBusWrapperBuilder budBuilder, IConfigurationSection serviceBus)
    {
        budBuilder.AddQueue<AuditEntryRecorded>(LogicalEntityNames.Queues.Audit)
            .Bind(serviceBus)
            .Bind(serviceBus.GetSection("Audit"))
            .PostConfigure(
                options => options.QueueName =
                    Environment.GetEnvironmentVariable(AuditQueueNameVariable) ?? options.QueueName
            );
    }

    private static void AddPrices(this YetAnotherBusWrapperBuilder busBuilder, IConfigurationSection serviceBus)
    {
        busBuilder.AddTopic<PriceChanged>(
                LogicalEntityNames.Topics.Prices,
                configureMessage: (serviceBusMessage, message) =>
                {
                    serviceBusMessage.MessageId = MessageIdSanitizer.Sanitize($"{message.ProductId}-{message.Price}");
                    serviceBusMessage.ApplicationProperties["productId"] = message.ProductId;
                }
            )
            .Bind(serviceBus)
            .Bind(serviceBus.GetSection("Prices"));
        busBuilder.AddTopicConsumer<PriceCacheConsumer, PriceChanged>(
                LogicalEntityNames.Subscriptions.PriceCache,
                options => options.DeadLetteringOnMessageExpiration = true
            )
            .Bind(serviceBus.GetSection("Prices:Cache"));
        busBuilder.AddTopicConsumer<PriceSearchIndexConsumer, PriceChanged>(
                LogicalEntityNames.Subscriptions.PriceSearch
            )
            .Bind(serviceBus.GetSection("Prices:Search"));
    }

    private static void AddCacheInvalidation(
        this YetAnotherBusWrapperBuilder busBuilder,
        IConfigurationSection serviceBus
    )
    {
        busBuilder.AddTopic<CacheInvalidated>(LogicalEntityNames.Topics.CacheInvalidation)
            .Bind(serviceBus)
            .Bind(serviceBus.GetSection("CacheInvalidation"));
        busBuilder.AddTopicConsumer<CacheInvalidatedConsumer, CacheInvalidated>(
                LogicalEntityNames.Subscriptions.LocalCache,
                options => options.AutoDeleteOnIdle = TimeSpan.FromHours(1)
            )
            .Bind(serviceBus.GetSection("CacheInvalidation:LocalCache"));
    }

    private static void AddNotifications(this YetAnotherBusWrapperBuilder busBuilder, IConfigurationSection serviceBus)
    {
        busBuilder.AddTopic<CustomerNotified>(LogicalEntityNames.Topics.Notifications)
            .Bind(serviceBus)
            .Bind(serviceBus.GetSection("Notifications"));
        busBuilder.AddTopicConsumer<CustomerNotifiedConsumer, CustomerNotified>(LogicalEntityNames.Subscriptions.Email)
            .Bind(serviceBus.GetSection("Notifications:Email"));
    }
}
