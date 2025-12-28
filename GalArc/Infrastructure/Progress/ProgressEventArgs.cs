using System;

namespace GalArc.Infrastructure.Progress;

internal class ProgressEventArgs : EventArgs
{
    public int Value;
    public int Max;
    public ProgressAction Action;
}
