using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RetriesAndDeadLettering.Data.TestCases;

internal class T03BodyIsNotValidJson : IRetriesAndDeadLetteringTestCase
{
    public int TestCaseId => 3;

    public int MaxDeliveryCount => 2;

    public ConsumerFailurePlan FailurePlan => ConsumerFailurePlan.Never;

    public IReadOnlyList<OrderPlaced> Messages => [];

    public IReadOnlyList<RawMessageDto> RawMessages => [new("raw-001", "{\"OrderId\": \"order-001\", ")];

    public int DeliveriesToWaitFor => 0;

    public int DeadLetteredMessagesToWaitFor => 1;
}
