using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sending.Data;

public interface ISendingTestCase
{
    int TestCaseId { get; }
    OrderPlaced Message { get; }
    void ConfigureBus(YetAnotherBusWrapperBuilder builder);
    void ConfigureMessage(ServiceBusMessage serviceBusMessage, OrderPlaced message);
}
