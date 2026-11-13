using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T02MissingQueueName : IOptionsValidationTestCase
{
    public int TestCaseId => 2;

    public void Configure(YetAnotherBusWrapperBuilder builder, TestCaseScope scope)
    {
        builder.AddQueue<OrderPlaced>("orders").Configure(options => options.ConnectionString = scope.ConnectionString);
    }
}
