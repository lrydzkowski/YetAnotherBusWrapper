using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sending.Data.TestCases;

internal class T03ReplacedMessageSerializer : ISendingTestCase
{
    public int TestCaseId => 3;

    public OrderPlaced Message => new("order-003", 7, OrderStatus.Paid);

    public void ConfigureBus(YetAnotherBusWrapperBuilder builder)
    {
        builder.UseMessageSerializer<PlainTextMessageSerializer>();
    }

    public void ConfigureMessage(ServiceBusMessage serviceBusMessage, OrderPlaced message)
    {
    }
}
