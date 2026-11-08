using Azure.Messaging.ServiceBus.Administration;

namespace YetAnotherBusWrapper.Registration;

internal sealed record SubscriptionRegistration(
    string TopicLogicalEntityName,
    string SubscriptionLogicalName,
    Action<CreateSubscriptionOptions>? ConfigureCreateOptions
)
{
    public string LogicalEntityName { get; } = GetLogicalEntityName(TopicLogicalEntityName, SubscriptionLogicalName);

    public static string GetLogicalEntityName(string topicLogicalEntityName, string subscriptionLogicalName)
    {
        return $"{topicLogicalEntityName}/{subscriptionLogicalName}";
    }
}
