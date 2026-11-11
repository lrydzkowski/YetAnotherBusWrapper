using YetAnotherBusWrapper.Tests.Integration.Common.Context;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsReload.Data;

public interface IOptionsReloadTestCase
{
    int TestCaseId { get; }
    int ExpectedDeliveries { get; }

    Dictionary<string, string?> GetInitialOptions(TestCaseScope scope);

    Dictionary<string, string?> GetChangedOptions(TestCaseScope scope);

    Task PrepareAsync(TestCaseScope scope, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
