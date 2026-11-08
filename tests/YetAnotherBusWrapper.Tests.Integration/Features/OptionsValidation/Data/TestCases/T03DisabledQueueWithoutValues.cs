using YetAnotherBusWrapper.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T03DisabledQueueWithoutValues : IOptionsValidationTestCase
{
    public int TestCaseId => 3;

    public void ConfigureQueue(QueueOptions options, TestCaseScope scope)
    {
        options.Enabled = false;
    }
}
