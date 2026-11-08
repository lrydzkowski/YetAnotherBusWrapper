namespace YetAnotherBusWrapper.Tests.Integration.Common.Logging;

public record CapturedScopedLogDto(
    string Level,
    string Logger,
    string Message,
    string? ExceptionType,
    List<Dictionary<string, object?>> Scopes
);
