namespace YetAnotherBusWrapper.Tests.Integration.Features.Sessions.Data;

public record SessionDeliveriesDto(string? SessionId, List<SessionDeliveryDto> Deliveries);
