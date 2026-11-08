namespace YetAnotherBusWrapper.Tests.Integration.Common.Messages;

public record OrderPlaced(string OrderId, int Quantity, OrderStatus Status);

public enum OrderStatus
{
    New,
    Paid
}
