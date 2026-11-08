using Azure.Messaging.ServiceBus;
using YetAnotherBusWrapper.Sending;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sending.Data.TestCases;

internal class T08SanitizedMessageIdWithNonAsciiLetters : ISendingTestCase
{
    public int TestCaseId => 8;

    public OrderPlaced Message => new("Zamówienie-ŁÓDŹ/008", 1, OrderStatus.New);

    public void ConfigureBus(YetAnotherBusWrapperBuilder builder)
    {
    }

    public void ConfigureMessage(ServiceBusMessage serviceBusMessage, OrderPlaced message)
    {
        serviceBusMessage.MessageId = MessageIdSanitizer.Sanitize(message.OrderId);
    }
}
