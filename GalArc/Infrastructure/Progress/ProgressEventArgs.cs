using System;

namespace GalArc.Infrastructure.Progress;

public class ProgressEventArgs : EventArgs
{
    public int Value;
    public int Max;
    public ProgressAction Action;
}
