using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.Options;

internal sealed class SubscriptionOptionsValidator : IValidateOptions<SubscriptionOptions>
{
    private readonly YetAnotherBusWrapperRegistry _registry;
    private readonly IOptionsFactory<TopicOptions> _topicOptionsFactory;

    public SubscriptionOptionsValidator(
        YetAnotherBusWrapperRegistry registry,
        IOptionsFactory<TopicOptions> topicOptionsFactory
    )
    {
        _registry = registry;
        _topicOptionsFactory = topicOptionsFactory;
    }

    public ValidateOptionsResult Validate(string? name, SubscriptionOptions options)
    {
        if (!options.Enabled || IsTopicDisabled(name))
        {
            return ValidateOptionsResult.Skip;
        }

        List<string> failures = [];
        if (string.IsNullOrWhiteSpace(options.SubscriptionName))
        {
            failures.Add($"Subscription '{name}' requires {nameof(SubscriptionOptions.SubscriptionName)}.");
        }

        string entity = $"Subscription '{name}'";
        failures.AddRange(OptionValueRules.GetMaxDeliveryCountFailures(entity, options.MaxDeliveryCount));
        failures.AddRange(OptionValueRules.GetConsumerProcessingFailures(entity, options));

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private bool IsTopicDisabled(string? name)
    {
        SubscriptionRegistration? registration = name is null ? null : _registry.FindSubscription(name);

        return registration is not null && !_topicOptionsFactory.Create(registration.TopicLogicalEntityName).Enabled;
    }
}
