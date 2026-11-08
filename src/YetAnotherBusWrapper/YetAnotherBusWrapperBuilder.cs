using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Consuming;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Registration;
using YetAnotherBusWrapper.Sending;
using YetAnotherBusWrapper.Serialization;

namespace YetAnotherBusWrapper;

public sealed class YetAnotherBusWrapperBuilder
{
    private readonly YetAnotherBusWrapperRegistry _registry;

    internal YetAnotherBusWrapperBuilder(IServiceCollection services, YetAnotherBusWrapperRegistry registry)
    {
        Services = services;
        _registry = registry;
    }

    public IServiceCollection Services { get; }

    public OptionsBuilder<QueueOptions> AddQueue<TMessage>(
        string logicalEntityName,
        Action<CreateQueueOptions>? configureCreateOptions = null,
        Action<ServiceBusMessage, TMessage>? configureMessage = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalEntityName);

        _registry.AddQueue(new QueueRegistration(typeof(TMessage), logicalEntityName, configureCreateOptions));
        AddSender(
            EntityKind.Queue,
            logicalEntityName,
            senderFactory => senderFactory.GetQueueState(logicalEntityName),
            senderFactory => senderFactory.CreateQueueSender(logicalEntityName),
            configureMessage
        );

        return Services.AddOptions<QueueOptions>(logicalEntityName).ValidateOnStart();
    }

    public OptionsBuilder<TopicOptions> AddTopic<TMessage>(
        string logicalEntityName,
        Action<CreateTopicOptions>? configureCreateOptions = null,
        Action<ServiceBusMessage, TMessage>? configureMessage = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalEntityName);

        _registry.AddTopic(new TopicRegistration(typeof(TMessage), logicalEntityName, configureCreateOptions));
        AddSender(
            EntityKind.Topic,
            logicalEntityName,
            senderFactory => senderFactory.GetTopicState(logicalEntityName),
            senderFactory => senderFactory.CreateTopicSender(logicalEntityName),
            configureMessage
        );

        return Services.AddOptions<TopicOptions>(logicalEntityName).ValidateOnStart();
    }

    public OptionsBuilder<QueueConsumerOptions> AddQueueConsumer<TConsumer, TMessage>()
        where TConsumer : class, IMessageConsumer<TMessage>
    {
        string logicalEntityName = _registry.GetQueueLogicalEntityName(typeof(TMessage));
        _registry.AddQueueConsumer(logicalEntityName, typeof(TConsumer));
        AddConsumer<TConsumer, TMessage>(
            EntityKind.Queue,
            logicalEntityName,
            processorFactory => processorFactory.IsQueueEnabled(logicalEntityName),
            (processorFactory, handlers) => processorFactory.CreateQueueProcessor(logicalEntityName, handlers)
        );

        return Services.AddOptions<QueueConsumerOptions>(logicalEntityName);
    }

    public OptionsBuilder<SubscriptionOptions> AddTopicConsumer<TConsumer, TMessage>(
        string subscriptionLogicalName,
        Action<CreateSubscriptionOptions>? configureCreateOptions = null
    )
        where TConsumer : class, IMessageConsumer<TMessage>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionLogicalName);

        string topicLogicalEntityName = _registry.GetTopicLogicalEntityName(typeof(TMessage));
        SubscriptionRegistration registration = new(
            topicLogicalEntityName,
            subscriptionLogicalName,
            configureCreateOptions
        );
        _registry.AddSubscription(registration);
        AddConsumer<TConsumer, TMessage>(
            EntityKind.Subscription,
            registration.LogicalEntityName,
            processorFactory => processorFactory.IsSubscriptionEnabled(
                topicLogicalEntityName,
                registration.LogicalEntityName
            ),
            (processorFactory, handlers) => processorFactory.CreateSubscriptionProcessor(
                topicLogicalEntityName,
                registration.LogicalEntityName,
                handlers
            )
        );

        return Services.AddOptions<SubscriptionOptions>(registration.LogicalEntityName).ValidateOnStart();
    }

    public YetAnotherBusWrapperBuilder ConfigureJsonSerializerOptions(Action<JsonSerializerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.Configure<MessageSerializerOptions>(options => configure(options.JsonSerializerOptions));

        return this;
    }

    public YetAnotherBusWrapperBuilder UseMessageSerializer<TSerializer>()
        where TSerializer : class, IMessageSerializer
    {
        Services.Replace(ServiceDescriptor.Singleton<IMessageSerializer, TSerializer>());

        return this;
    }

    public YetAnotherBusWrapperBuilder UseInstanceNameProvider<TProvider>()
        where TProvider : class, IInstanceNameProvider
    {
        Services.Replace(ServiceDescriptor.Singleton<IInstanceNameProvider, TProvider>());

        return this;
    }

    public YetAnotherBusWrapperBuilder UseInstanceNameProvider(IInstanceNameProvider instanceNameProvider)
    {
        ArgumentNullException.ThrowIfNull(instanceNameProvider);

        Services.Replace(ServiceDescriptor.Singleton(instanceNameProvider));

        return this;
    }

    public YetAnotherBusWrapperBuilder DisableHostedServices()
    {
        _registry.DisableHostedServices();

        return this;
    }

    private void AddSender<TMessage>(
        EntityKind entityKind,
        string logicalEntityName,
        Func<ServiceBusSenderFactory, SenderEntityState> getEntityState,
        Func<ServiceBusSenderFactory, ServiceBusSender> createSender,
        Action<ServiceBusMessage, TMessage>? configureMessage
    )
    {
        Services.AddSingleton<IMessageSender<TMessage>>(
            serviceProvider => new MessageSender<TMessage>(
                entityKind,
                logicalEntityName,
                () => getEntityState(serviceProvider.GetRequiredService<ServiceBusSenderFactory>()),
                () => createSender(serviceProvider.GetRequiredService<ServiceBusSenderFactory>()),
                serviceProvider.GetRequiredService<IMessageSerializer>(),
                configureMessage,
                serviceProvider.GetRequiredService<ILogger<MessageSender<TMessage>>>()
            )
        );
    }

    private void AddConsumer<TConsumer, TMessage>(
        EntityKind entityKind,
        string logicalEntityName,
        Func<ServiceBusProcessorFactory, bool> isEntityEnabled,
        Func<ServiceBusProcessorFactory, ProcessorHandlers, IProcessorAdapter> createProcessor
    )
        where TConsumer : class, IMessageConsumer<TMessage>
    {
        ConsumerRegistration registration = new(typeof(TMessage), typeof(TConsumer), logicalEntityName);
        Services.TryAddScoped<TConsumer>();
        Services.AddSingleton<IConsumerProcessor>(
            serviceProvider => new ConsumerProcessor(
                entityKind,
                logicalEntityName,
                () => isEntityEnabled(serviceProvider.GetRequiredService<ServiceBusProcessorFactory>()),
                handlers => createProcessor(serviceProvider.GetRequiredService<ServiceBusProcessorFactory>(), handlers),
                ActivatorUtilities.CreateInstance<ReceivedMessageDispatcher<TMessage>>(serviceProvider, registration),
                serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                serviceProvider.GetRequiredService<ILogger<ConsumerProcessor>>()
            )
        );
    }
}
