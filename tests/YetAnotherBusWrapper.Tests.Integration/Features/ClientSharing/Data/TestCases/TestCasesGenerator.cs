namespace YetAnotherBusWrapper.Tests.Integration.Features.ClientSharing.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IClientSharingTestCase> Generate()
    {
        yield return new T01DefaultRetryOptionsWithManyRegistrations();
        yield return new T02SameCustomRetryOptions();
        yield return new T03DifferentRetryOptions();
        yield return new T04DifferentRetryOptionsWithHostedServicesDisabled();
    }
}
