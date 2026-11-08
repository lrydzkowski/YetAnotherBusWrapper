namespace YetAnotherBusWrapper.Abstractions;

public sealed class SendOptions
{
    public DateTimeOffset? ScheduledEnqueueTime { get; init; }
}
