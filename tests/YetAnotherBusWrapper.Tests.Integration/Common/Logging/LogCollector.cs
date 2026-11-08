using System.Collections.Concurrent;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Logging;

public sealed class LogCollector
{
    private readonly ConcurrentQueue<CapturedScopedLogDto> _entries = new();

    public void Add(CapturedScopedLogDto entry)
    {
        _entries.Enqueue(entry);
    }

    public List<CapturedLogDto> GetLogs()
    {
        return _entries.Select(entry => new CapturedLogDto(entry.Level, entry.Logger, entry.Message)).ToList();
    }

    public List<CapturedScopedLogDto> GetLogsWithScopes()
    {
        return _entries.ToList();
    }
}
