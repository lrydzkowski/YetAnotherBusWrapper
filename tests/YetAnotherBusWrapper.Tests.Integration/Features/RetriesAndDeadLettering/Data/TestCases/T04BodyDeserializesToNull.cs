using YetAnotherBusWrapper.Tests.Integration.Common.Consumers;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.RetriesAndDeadLettering.Data.TestCases;

internal class T04BodyDeserializesToNull : IRetriesAndDeadLetteringTestCase
{
    public int TestCaseId => 4;

    public int MaxDeliveryCount => 2;

    public ConsumerFailurePlan FailurePlan => ConsumerFailurePlan.Never;

    public IReadOnlyList<OrderPlaced> Messages => [];

    public IReadOnlyList<RawMessageDto> RawMessages => [new("raw-001", "null")];

    public int DeliveriesToWaitFor => 0;

    public int DeadLetteredMessagesToWaitFor => 1;
}
