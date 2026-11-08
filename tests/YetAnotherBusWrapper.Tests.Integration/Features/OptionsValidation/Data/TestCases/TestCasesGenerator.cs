namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IOptionsValidationTestCase> Generate()
    {
        yield return new T01MissingConnectionString();
        yield return new T02MissingQueueName();
        yield return new T03DisabledQueueWithoutValues();
    }
}
