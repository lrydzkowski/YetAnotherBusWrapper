namespace YetAnotherBusWrapper.Sample.Api.Features.Payments;

public sealed record PaymentRequested(string PaymentId, decimal Amount, int FailuresToSimulate);
