using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicConsuming.Data;

public interface ITopicConsumingTestCase
{
    int TestCaseId { get; }
    IReadOnlyList<OrderPlaced> Messages { get; }
    void ConfigureMessage(ServiceBusMessage serviceBusMessage, OrderPlaced message);
}
