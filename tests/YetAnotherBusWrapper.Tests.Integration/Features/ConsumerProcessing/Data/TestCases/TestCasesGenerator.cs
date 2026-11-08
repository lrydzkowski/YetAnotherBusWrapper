namespace YetAnotherBusWrapper.Tests.Integration.Features.ConsumerProcessing.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IConsumerProcessingTestCase> Generate()
    {
        yield return new T01DefaultOptions();
        yield return new T02QueueMaxConcurrentCalls();
        yield return new T03SubscriptionMaxConcurrentCalls();
        yield return new T04LockRenewedByDefault();
        yield return new T05LockRenewalDisabled();
    }
}
