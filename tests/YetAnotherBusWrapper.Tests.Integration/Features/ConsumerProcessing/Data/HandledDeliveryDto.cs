using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.ConsumerProcessing.Data;

public record HandledDeliveryDto(OrderPlaced Message, int DeliveryCount);
