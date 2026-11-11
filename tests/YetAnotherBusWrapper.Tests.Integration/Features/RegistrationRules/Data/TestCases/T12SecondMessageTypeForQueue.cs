using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RegistrationRules.Data.TestCases;

internal class T12SecondMessageTypeForQueue : IRegistrationRulesTestCase
{
    public int TestCaseId => 12;

    public void Register(YetAnotherBusWrapperBuilder builder)
    {
        builder.AddQueue<OrderPlaced>("orders");
        builder.AddQueue<ReportGenerated>("orders");
    }
}
