using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Correlation.Data;

public interface ICorrelationTestCase
{
    int TestCaseId { get; }
    ConsumerFailurePlan FailurePlan { get; }
    IReadOnlyList<OrderPlaced> Messages { get; }
    Action<ServiceBusMessage, OrderPlaced>? ConfigureMessage { get; }
    int DeliveriesToWaitFor { get; }
}
