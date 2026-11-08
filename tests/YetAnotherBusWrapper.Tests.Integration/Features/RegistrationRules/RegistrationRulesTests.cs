using Microsoft.Extensions.Hosting;
using YetAnotherBusWrapper.Tests.Integration.Common.Host;
using YetAnotherBusWrapper.Tests.Integration.Common.Logging;
using YetAnotherBusWrapper.Tests.Integration.Common.Models;
using YetAnotherBusWrapper.Tests.Integration.Common.Services;
using YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data;
using YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules;

public class RegistrationRulesTests
{
    private readonly VerifySettings _verifySettings = VerifySettingsBuilder.Build();

    public static IEnumerable<object[]> TestCases =>
        TestCasesGenerator.Generate().Select(testCase => (object[])[testCase]);

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task RegistrationRules_ShouldRejectMisconfiguredRegistrations(IRegistrationRulesTestCase testCase)
    {
        RegistrationRulesTestResult result = Register(testCase);

        await Verify(result, _verifySettings).UseParameters(testCase.TestCaseId.ToString("D3"));
    }

    private static RegistrationRulesTestResult Register(IRegistrationRulesTestCase testCase)
    {
        LogCollector logs = new();
        bool registered = false;
        ThrownException? thrownException = null;
        try
        {
            using IHost host = TestHostBuilder.Build(
                logs,
                services => services.AddYetAnotherBusWrapper(testCase.Register)
            );
            registered = true;
        }
        catch (Exception exception)
        {
            thrownException = ThrownException.From(exception);
        }

        return new RegistrationRulesTestResult(testCase.TestCaseId, registered, thrownException);
    }
}

internal record RegistrationRulesTestResult(int TestCaseId, bool Registered, ThrownException? ThrownException);
