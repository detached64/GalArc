using System;

namespace GalArc.Infrastructure.Logging;

internal sealed class LogEventArgs(LogEntry entry) : EventArgs
{
    public LogEntry Entry { get; } = entry;
}
