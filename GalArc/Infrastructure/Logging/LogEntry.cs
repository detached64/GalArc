using System;

namespace GalArc.Infrastructure.Logging;

internal sealed class LogEntry(string message, DateTime time, LogLevel level)
{
    public string Message { get; } = message;
    public DateTime Timestamp { get; } = time;
    public LogLevel Level { get; } = level;

    public override string ToString()
    {
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}][{Level}] {Message}";
    }
}
