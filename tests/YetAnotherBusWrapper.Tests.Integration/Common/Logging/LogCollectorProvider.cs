using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Logging;

public sealed class LogCollectorProvider : ILoggerProvider, ISupportExternalScope
{
    private const string CapturedCategoryPrefix = "YetAnotherBusWrapper.";
    private const string IgnoredCategoryPrefix = "YetAnotherBusWrapper.Tests.";
    private const string CapturedTestConsumerCategoryPrefix =
        "YetAnotherBusWrapper.Tests.Integration.Common.Consumers.";

    private readonly LogCollector _collector;
    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

    public LogCollectorProvider(LogCollector collector)
    {
        _collector = collector;
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (!IsCaptured(categoryName))
        {
            return NullLogger.Instance;
        }

        return new CollectingLogger(categoryName, _collector, () => _scopeProvider);
    }

    public void Dispose()
    {
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    private static bool IsCaptured(string categoryName)
    {
        if (categoryName.StartsWith(CapturedTestConsumerCategoryPrefix, StringComparison.Ordinal))
        {
            return true;
        }

        return categoryName.StartsWith(CapturedCategoryPrefix, StringComparison.Ordinal)
            && !categoryName.StartsWith(IgnoredCategoryPrefix, StringComparison.Ordinal);
    }
}
