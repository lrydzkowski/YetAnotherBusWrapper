using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Sending;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicConsuming.Data.TestCases;

internal class T03MessagePropertiesFromContent : ITopicConsumingTestCase
{
    public int TestCaseId => 3;

    public IReadOnlyList<OrderPlaced> Messages => [new("Order-003", 4, OrderStatus.Paid)];

    public void ConfigureMessage(ServiceBusMessage serviceBusMessage, OrderPlaced message)
    {
        serviceBusMessage.MessageId = MessageIdSanitizer.Sanitize(message.OrderId);
        serviceBusMessage.ApplicationProperties["status"] = message.Status.ToString();
    }
}
