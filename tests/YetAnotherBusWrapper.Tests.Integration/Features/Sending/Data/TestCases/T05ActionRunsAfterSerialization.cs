using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sending.Data.TestCases;

internal class T05ActionRunsAfterSerialization : ISendingTestCase
{
    public int TestCaseId => 5;

    public OrderPlaced Message => new("order-005", 4, OrderStatus.New);

    public void ConfigureBus(YetAnotherBusWrapperBuilder builder)
    {
    }

    public void ConfigureMessage(ServiceBusMessage serviceBusMessage, OrderPlaced message)
    {
        serviceBusMessage.ApplicationProperties["serializedContentType"] = serviceBusMessage.ContentType;
        serviceBusMessage.ContentType = "application/vnd.order-placed+json";
    }
}
