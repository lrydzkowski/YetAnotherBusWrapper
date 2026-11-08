using YetAnotherBusWrapper.Abstractions;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Consumers;

public record RecordedDelivery<TMessage>(int ConsumerInstance, TMessage Message, MessageContext Context);
