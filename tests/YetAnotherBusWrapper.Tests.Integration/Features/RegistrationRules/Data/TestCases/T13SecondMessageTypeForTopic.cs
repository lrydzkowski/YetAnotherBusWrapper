using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T13SecondMessageTypeForTopic : IRegistrationRulesTestCase
{
    public int TestCaseId => 13;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddTopic<OrderPlaced>("orders");
        builder.AddTopic<ReportGenerated>("orders");
    }
}
