using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T01MissingConnectionString : IOptionsValidationTestCase
{
    public int TestCaseId => 1;

    public void ConfigureQueue(QueueOptions options, TestCaseScope scope)
    {
        options.QueueName = scope.GetQueueName("orders");
    }
}
