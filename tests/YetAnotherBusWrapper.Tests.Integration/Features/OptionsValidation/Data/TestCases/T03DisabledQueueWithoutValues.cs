using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T03DisabledQueueWithoutValues : IOptionsValidationTestCase
{
    public int TestCaseId => 3;

    public void Configure(YetAnotherBusWrapperBuilder builder, TestCaseScope scope)
    {
        builder.AddQueue<OrderPlaced>("orders").Configure(options => options.Enabled = false);
    }
}
