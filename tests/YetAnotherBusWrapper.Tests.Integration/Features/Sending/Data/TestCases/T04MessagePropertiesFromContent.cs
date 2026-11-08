using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Sending;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sending.Data.TestCases;

internal class T04MessagePropertiesFromContent : ISendingTestCase
{
    public int TestCaseId => 4;

    public OrderPlaced Message => new("Order_ABC-004/eu", 2, OrderStatus.Paid);

    public void ConfigureBus(YetAnotherBusWrapperBuilder builder)
    {
    }

    public void ConfigureMessage(ServiceBusMessage serviceBusMessage, OrderPlaced message)
    {
        serviceBusMessage.MessageId = MessageIdSanitizer.Sanitize(message.OrderId);
        serviceBusMessage.SessionId = message.OrderId;
        serviceBusMessage.ApplicationProperties["status"] = message.Status.ToString();
        serviceBusMessage.ApplicationProperties["quantity"] = message.Quantity;
    }
}
