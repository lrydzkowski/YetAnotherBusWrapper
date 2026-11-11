namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsReload.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IOptionsReloadTestCase> Generate()
    {
        yield return new T01QueueNameChangedAfterStart();
        yield return new T02QueueDisabledAfterStart();
        yield return new T03QueueEnabledAfterStart();
    }
}
