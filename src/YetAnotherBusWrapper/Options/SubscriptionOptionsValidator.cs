using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Registration;

namespace YetAnotherBusWrapper.Options;

internal sealed class SubscriptionOptionsValidator : IValidateOptions<SubscriptionOptions>
{
    private readonly YetAnotherBusWrapperRegistry _registry;
    private readonly IOptionsMonitor<TopicOptions> _topicOptionsMonitor;

    public SubscriptionOptionsValidator(
        YetAnotherBusWrapperRegistry registry,
        IOptionsMonitor<TopicOptions> topicOptionsMonitor
    )
    {
        _registry = registry;
        _topicOptionsMonitor = topicOptionsMonitor;
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

        return registration is not null && !_topicOptionsMonitor.Get(registration.TopicLogicalEntityName).Enabled;
    }
}
