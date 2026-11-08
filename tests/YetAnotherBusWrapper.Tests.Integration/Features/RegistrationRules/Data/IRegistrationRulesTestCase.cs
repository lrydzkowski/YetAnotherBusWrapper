namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data;

public interface IRegistrationRulesTestCase
{
    int TestCaseId { get; }
    void Register(YetAnotherBusWrapperBuilder builder);
}
