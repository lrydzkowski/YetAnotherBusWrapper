namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IOptionsValidationTestCase> Generate()
    {
        yield return new T01MissingConnectionString();
        yield return new T02MissingQueueName();
        yield return new T03DisabledQueueWithoutValues();
        yield return new T04ValuesFromRegisteredServices();
        yield return new T05QueueWithInvalidValues();
        yield return new T06QueueConsumerWithInvalidValues();
        yield return new T07TopicWithInvalidValues();
        yield return new T08SubscriptionWithInvalidValues();
        yield return new T09DisabledQueueWithInvalidValues();
        yield return new T10InvalidQueueAndQueueConsumer();
        yield return new T11BoundaryValues();
    }
}
