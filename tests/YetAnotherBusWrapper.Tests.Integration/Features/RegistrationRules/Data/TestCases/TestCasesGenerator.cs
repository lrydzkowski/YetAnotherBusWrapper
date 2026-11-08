namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IRegistrationRulesTestCase> Generate()
    {
        yield return new T01SecondQueueForMessageType();
        yield return new T02TopicForMessageTypeWithQueue();
        yield return new T03QueueForMessageTypeWithTopic();
        yield return new T04SecondTopicForMessageType();
        yield return new T05QueueConsumerWithoutEntity();
        yield return new T06TopicConsumerWithoutEntity();
        yield return new T07QueueConsumerForTopic();
        yield return new T08TopicConsumerForQueue();
        yield return new T09SameSubscriptionTwice();
        yield return new T10ValidRegistrations();
        yield return new T11SecondQueueConsumer();
    }
}
