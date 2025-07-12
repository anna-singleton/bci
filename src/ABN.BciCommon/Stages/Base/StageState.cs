using System.Diagnostics;

namespace ABN.BciCommon.Stages.Base;

public enum StageState
{
    NotStarted,
    Skipped,
    InProgress,
    CompletedSuccess,
    CompletedFailed
}

public static class StageStateExtensions
{
    public static bool IsComplete(this StageState state)
    {
        return state switch
        {
            StageState.NotStarted or StageState.Skipped or StageState.InProgress => false,
            StageState.CompletedSuccess or StageState.CompletedFailed => true,
            _ => throw new UnreachableException()
        };
    }

    public static bool IsCompleteOrSkipped(this StageState state)
    {
        return state switch
        {
            StageState.NotStarted or StageState.InProgress => false,
            StageState.CompletedSuccess or StageState.CompletedFailed or StageState.Skipped => true,
            _ => throw new UnreachableException()
        };
    }
}