namespace YetAnotherBusWrapper.Tests.Integration.Features.Provisioning.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IProvisioningTestCase> Generate()
    {
        yield return new T01CreateOnStartupNotSet();
        yield return new T02CreateOnStartupWithDefaults();
        yield return new T03ExistingQueueLeftUnchanged();
        yield return new T04MaxDeliveryCountAndDuplicateDetection();
        yield return new T05CreateOptionsCallback();
        yield return new T06DeleteOnShutdown();
        yield return new T07DeletionFailure();
        yield return new T08ProvisioningFailureContinuesStartup();
        yield return new T09ProvisioningFailureFailsStartup();
        yield return new T10UnresolvableQueueName();
        yield return new T11QueueCreatedDuringProvisioning();
    }
}
