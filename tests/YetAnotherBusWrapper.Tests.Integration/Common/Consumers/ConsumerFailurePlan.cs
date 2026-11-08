namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public record ConsumerFailurePlan(int FailingDeliveryCount)
{
    public static ConsumerFailurePlan Never { get; } = new(0);

    public static ConsumerFailurePlan Always { get; } = new(int.MaxValue);

    public bool ShouldFail(int deliveryCount)
    {
        return deliveryCount <= FailingDeliveryCount;
    }
}
