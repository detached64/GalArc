using System;

namespace GalArc.Infrastructure.Progress;

internal static class ProgressManager
{
    public static event EventHandler<ProgressEventArgs> ProgressEvent;

    public static event EventHandler<ProgressEventArgs> OverallProgressEvent;

    public static void Progress() => ProgressEvent?.Invoke(null, new ProgressEventArgs { Action = ProgressAction.Progress });
    public static void SetMax(int max) => ProgressEvent?.Invoke(null, new ProgressEventArgs { Action = ProgressAction.SetMax, Max = max });
    public static void SetValue(int value) => ProgressEvent?.Invoke(null, new ProgressEventArgs { Action = ProgressAction.SetValue, Value = value });
    public static void Finish() => ProgressEvent?.Invoke(null, new ProgressEventArgs { Action = ProgressAction.Finish });

    public static void OverallProgress() => OverallProgressEvent?.Invoke(null, new ProgressEventArgs { Action = ProgressAction.Progress });
    public static void OverallSetMax(int max) => OverallProgressEvent?.Invoke(null, new ProgressEventArgs { Action = ProgressAction.SetMax, Max = max });
    public static void OverallSetValue(int value) => OverallProgressEvent?.Invoke(null, new ProgressEventArgs { Action = ProgressAction.SetValue, Value = value });
    public static void OverallFinish() => OverallProgressEvent?.Invoke(null, new ProgressEventArgs { Action = ProgressAction.Finish });
}
