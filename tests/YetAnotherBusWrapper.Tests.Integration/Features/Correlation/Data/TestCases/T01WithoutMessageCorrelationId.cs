using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Correlation.Data.TestCases;

internal class T01WithoutMessageCorrelationId : ICorrelationTestCase
{
    public int TestCaseId => 1;

    public ConsumerFailurePlan FailurePlan => ConsumerFailurePlan.Never;

    public IReadOnlyList<OrderPlaced> Messages =>
    [
        new("order-001", 3, OrderStatus.Paid),
        new("order-002", 1, OrderStatus.New)
    ];

    public Action<ServiceBusMessage, OrderPlaced>? ConfigureMessage => null;

    public int DeliveriesToWaitFor => 2;
}
