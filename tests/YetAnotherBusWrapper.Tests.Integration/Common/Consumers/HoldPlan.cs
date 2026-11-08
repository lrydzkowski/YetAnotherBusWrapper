namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public record HoldPlan(int TargetConcurrency, TimeSpan Timeout)
{
    public static HoldPlan For(TimeSpan timeout)
    {
        return new HoldPlan(int.MaxValue, timeout);
    }
}
