namespace YetAnotherBusWrapper.Naming;

internal static class EntityKindExtensions
{
    public static string ToLogName(this EntityKind kind)
    {
        return kind switch
        {
            EntityKind.Queue => "queue",
            EntityKind.Topic => "topic",
            EntityKind.Subscription => "subscription",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown entity kind.")
        };
    }
}
