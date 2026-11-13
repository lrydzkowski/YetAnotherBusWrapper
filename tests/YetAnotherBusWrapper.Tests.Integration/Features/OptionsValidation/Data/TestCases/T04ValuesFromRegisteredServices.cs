using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Tests.Integration.Common.Context;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.OptionsValidation.Data.TestCases;

internal class T04ValuesFromRegisteredServices : IOptionsValidationTestCase
{
    public int TestCaseId => 4;

    public void Configure(YetAnotherBusWrapperBuilder builder, TestCaseScope scope)
    {
        builder.Services.Configure<QueueConnectionValues>(
            values =>
            {
                values.ConnectionString = scope.ConnectionString;
                values.QueueName = scope.GetQueueName("orders");
            }
        );
        builder.AddQueue<OrderPlaced>("orders")
            .Configure<IOptions<QueueConnectionValues>>(
                (options, values) =>
                {
                    options.ConnectionString = values.Value.ConnectionString;
                    options.QueueName = values.Value.QueueName;
                }
            );
    }
}
