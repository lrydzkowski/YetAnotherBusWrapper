namespace YetAnotherBusWrapper.Abstractions;

public sealed record SendResult(SendStatus Status, IReadOnlyList<string> MessageIds);
