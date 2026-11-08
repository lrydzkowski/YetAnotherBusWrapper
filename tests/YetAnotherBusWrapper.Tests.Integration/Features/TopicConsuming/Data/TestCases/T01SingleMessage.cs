using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicConsuming.Data.TestCases;

internal class T01SingleMessage : ITopicConsumingTestCase
{
    public int TestCaseId => 1;

    public IReadOnlyList<OrderPlaced> Messages => [new("order-001", 2, OrderStatus.New)];

    public void ConfigureMessage(ServiceBusMessage serviceBusMessage, OrderPlaced message)
    {
    }
}
