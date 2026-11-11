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

        if (string.IsNullOrWhiteSpace(options.SubscriptionName))
        {
            return ValidateOptionsResult.Fail(
                $"Subscription '{name}' requires {nameof(SubscriptionOptions.SubscriptionName)}."
            );
        }

        return ValidateOptionsResult.Success;
    }

    private bool IsTopicDisabled(string? name)
    {
        SubscriptionRegistration? registration = name is null ? null : _registry.FindSubscription(name);

        return registration is not null && !_topicOptionsFactory.Create(registration.TopicLogicalEntityName).Enabled;
    }
}
