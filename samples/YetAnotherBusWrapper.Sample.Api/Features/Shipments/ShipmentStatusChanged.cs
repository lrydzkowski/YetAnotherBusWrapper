namespace YetAnotherBusWrapper.Sample.Api.Features.Shipments;

public sealed record ShipmentStatusChanged(string ShipmentId, int Sequence, string Status);
