using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Correlation.Data.TestCases;

internal class T03ConsumerFailsWithMessageCorrelationId : ICorrelationTestCase
{
    public int TestCaseId => 3;

    public ConsumerFailurePlan FailurePlan => new(1);

    public IReadOnlyList<OrderPlaced> Messages => [new("order-001", 3, OrderStatus.Paid)];

    public Action<ServiceBusMessage, OrderPlaced>? ConfigureMessage =>
        (serviceBusMessage, message) => serviceBusMessage.CorrelationId = $"correlation-{message.OrderId}";

    public int DeliveriesToWaitFor => 2;
}
