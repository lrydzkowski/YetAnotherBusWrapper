using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;

namespace YetAnotherBusWrapper.Tests.Integration.Features.TopicProvisioning.Data;

internal record TopicStateDto(TopicPropertiesDto Topic, List<SubscriptionPropertiesDto> Subscriptions);
