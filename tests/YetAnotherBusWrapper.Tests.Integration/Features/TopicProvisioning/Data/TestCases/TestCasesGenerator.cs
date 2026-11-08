namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<ITopicProvisioningTestCase> Generate()
    {
        yield return new T01CreateOnStartupWithDefaults();
        yield return new T02MaxDeliveryCountAndDuplicateDetection();
        yield return new T03CreateOptionsCallbacks();
        yield return new T04InstanceSuffix();
        yield return new T05DeleteOnShutdown();
        yield return new T06SubscriptionDeletedOnShutdown();
        yield return new T07ExistingEntitiesLeftUnchanged();
        yield return new T08SubscriptionProvisioningFailureFailsStartup();
        yield return new T09SubscriptionNameLongerThanLimit();
        yield return new T10ProvisioningFailureRollsBackCreatedEntities();
    }
}
