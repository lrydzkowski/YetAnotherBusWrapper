using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sending.Data.TestCases;

internal class T01SingleMessage : ISendingTestCase
{
    public int TestCaseId => 1;

    public OrderPlaced Message => new("order-001", 3, OrderStatus.Paid);

    public void ConfigureBus(YetAnotherBusWrapperBuilder builder)
    {
    }

    public void ConfigureMessage(ServiceBusMessage serviceBusMessage, OrderPlaced message)
    {
    }
}
