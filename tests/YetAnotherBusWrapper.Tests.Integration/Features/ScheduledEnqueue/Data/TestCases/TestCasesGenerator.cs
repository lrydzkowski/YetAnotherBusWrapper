namespace YetAnotherBusWrapper.Tests.Integration.Features.ScheduledEnqueue.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IScheduledEnqueueTestCase> Generate()
    {
        yield return new T01SingleMessage();
        yield return new T02Collection();
    }
}
