using Microsoft.Extensions.Logging;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Logging;

internal sealed class CollectingLogger : ILogger
{
    private const string NonStructuredScopeKey = "Scope";

    private readonly LogCollector _collector;
    private readonly Func<IExternalScopeProvider> _getScopeProvider;
    private readonly string _logger;

    public CollectingLogger(string category, LogCollector collector, Func<IExternalScopeProvider> getScopeProvider)
    {
        _logger = category.Split('.').Last();
        _collector = collector;
        _getScopeProvider = getScopeProvider;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return _getScopeProvider().Push(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        _collector.Add(
            new CapturedScopedLogDto(
                logLevel.ToString(),
                _logger,
                formatter(state, exception),
                exception?.GetType().Name,
                GetScopes()
            )
        );
    }

    private List<Dictionary<string, object?>> GetScopes()
    {
        List<Dictionary<string, object?>> scopes = [];
        _getScopeProvider().ForEachScope((scope, list) => list.Add(ToDictionary(scope)), scopes);

        return scopes;
    }

    private static Dictionary<string, object?> ToDictionary(object? scope)
    {
        if (scope is IEnumerable<KeyValuePair<string, object?>> properties)
        {
            return properties.ToDictionary(property => property.Key, property => property.Value);
        }

        return new Dictionary<string, object?> { [NonStructuredScopeKey] = scope?.ToString() };
    }
}
