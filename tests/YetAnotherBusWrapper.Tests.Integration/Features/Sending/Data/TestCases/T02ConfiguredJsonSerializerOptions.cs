using System.Text.Json;
using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sending.Data.TestCases;

internal class T02ConfiguredJsonSerializerOptions : ISendingTestCase
{
    public int TestCaseId => 2;

    public OrderPlaced Message => new("order-002", 5, OrderStatus.New);

    public void ConfigureBus(YetAnotherBusWrapperBuilder builder)
    {
        builder.ConfigureJsonSerializerOptions(
            options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        );
    }

    public void ConfigureMessage(ServiceBusMessage serviceBusMessage, OrderPlaced message)
    {
    }
}
