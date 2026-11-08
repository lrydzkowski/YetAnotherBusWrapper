using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T02MissingQueueName : IOptionsValidationTestCase
{
    public int TestCaseId => 2;

    public void ConfigureQueue(QueueOptions options, TestCaseScope scope)
    {
        options.ConnectionString = scope.ConnectionString;
    }
}
