using YetAnotherBusWrapper.Tests.Integration.Common.Context;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data;

public interface IOptionsValidationTestCase
{
    int TestCaseId { get; }
    void Configure(YetAnotherBusWrapperBuilder builder, TestCaseScope scope);
}
