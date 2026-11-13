namespace YetAnotherBusWrapper.Options;

internal static class OptionValueRules
{
    private const int MaxClientRetries = 100;

    private static readonly TimeSpan MaxDuplicateDetectionWindow = TimeSpan.FromDays(7);
    private static readonly TimeSpan MaxTryTimeout = TimeSpan.FromHours(1);
    private static readonly TimeSpan MinDuplicateDetectionWindow = TimeSpan.FromSeconds(20);

    public static IEnumerable<string> GetMaxDeliveryCountFailures(string entity, int maxDeliveryCount)
    {
        if (maxDeliveryCount < 1)
        {
            yield return $"{entity} has MaxDeliveryCount {maxDeliveryCount}, which must be at least 1.";
        }
    }

    public static IEnumerable<string> GetDuplicateDetectionWindowFailures(
        string entity,
        TimeSpan duplicateDetectionWindow
    )
    {
        if (duplicateDetectionWindow == TimeSpan.Zero)
        {
            yield break;
        }

        if (duplicateDetectionWindow < MinDuplicateDetectionWindow
            || duplicateDetectionWindow > MaxDuplicateDetectionWindow)
        {
            yield return $"{entity} has DuplicateDetectionWindow {duplicateDetectionWindow}, which must be zero or "
                + $"between {MinDuplicateDetectionWindow} and {MaxDuplicateDetectionWindow}.";
        }
    }

    public static IEnumerable<string> GetClientRetryFailures(string entity, ClientRetryOptions? clientRetry)
    {
        if (clientRetry is null)
        {
            yield return $"{entity} requires ClientRetry.";

            yield break;
        }

        if (clientRetry.MaxRetries is < 0 or > MaxClientRetries)
        {
            yield return $"{entity} has ClientRetry.MaxRetries {clientRetry.MaxRetries}, which must be between 0 and "
                + $"{MaxClientRetries}.";
        }

        if (clientRetry.TryTimeout < TimeSpan.Zero || clientRetry.TryTimeout > MaxTryTimeout)
        {
            yield return $"{entity} has ClientRetry.TryTimeout {clientRetry.TryTimeout}, which must be between "
                + $"{TimeSpan.Zero} and {MaxTryTimeout}.";
        }

        if (clientRetry.MaxDelay < TimeSpan.Zero)
        {
            yield return $"{entity} has ClientRetry.MaxDelay {clientRetry.MaxDelay}, which must not be negative.";
        }
    }

    public static IEnumerable<string> GetConsumerProcessingFailures(string entity, IConsumerProcessingOptions options)
    {
        if (options.MaxConcurrentCalls < 1)
        {
            yield return $"{entity} has MaxConcurrentCalls {options.MaxConcurrentCalls}, which must be at least 1.";
        }

        if (options.PrefetchCount < 0)
        {
            yield return $"{entity} has PrefetchCount {options.PrefetchCount}, which must not be negative.";
        }

        if (options.MaxAutoLockRenewalDuration < TimeSpan.Zero
            && options.MaxAutoLockRenewalDuration != Timeout.InfiniteTimeSpan)
        {
            yield return $"{entity} has MaxAutoLockRenewalDuration {options.MaxAutoLockRenewalDuration}, which must "
                + "not be negative, except for Timeout.InfiniteTimeSpan.";
        }

        if (options.MaxConcurrentSessions < 1)
        {
            yield return $"{entity} has MaxConcurrentSessions {options.MaxConcurrentSessions}, which must be at "
                + "least 1.";
        }

        if (options.SessionIdleTimeout <= TimeSpan.Zero)
        {
            yield return $"{entity} has SessionIdleTimeout {options.SessionIdleTimeout}, which must be greater than "
                + "zero.";
        }
    }
}
