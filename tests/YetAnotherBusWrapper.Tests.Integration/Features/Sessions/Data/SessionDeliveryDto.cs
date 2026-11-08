using YetAnotherBusWrapper.Abstractions;
using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Sessions.Data;

public record SessionDeliveryDto(OrderPlaced Message, MessageContext Context);
