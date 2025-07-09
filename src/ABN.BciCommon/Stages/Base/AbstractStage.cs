using Microsoft.Extensions.Logging;

namespace ABN.BciCommon.Stages.Base;

public abstract class AbstractStage(ILogger logger, string stageName)
{
    public StageState State { get; set; } = StageState.NotStarted;

    protected HashSet<string> ChildStageIds { get; } = [];

    protected HashSet<string> ParentStageIds { get; } = [];

    public string StageId { get; } = Guid.NewGuid().ToString();

    public string StageName { get; } = stageName;

    protected ILogger Logger { get; } = logger;

    protected virtual ValueTask<bool> ShouldExecuteAsync(PipelineContext context)
    {
        return ValueTask.FromResult(true);
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        using var logscope = Logger.BeginScope($"{StageName}.{StageId}");

        if (!await ShouldExecuteAsync(context))
        {
            State = StageState.Skipped;
            return;
        }

        State = StageState.InProgress;
        var result = await ProcessAsync(context);

        if (result == StageResult.Succeeded)
        {
            State = StageState.CompletedSuccess;
            return;
        }

        State = StageState.CompletedFailed;
        return;
    }

    public bool IsComplete => State.IsComplete();

    public void AddChildStage(AbstractStage childStage)
    {
        _ = ChildStageIds.Add(childStage.StageId);
        _ = childStage.ParentStageIds.Add(StageId);
    }

    protected abstract Task<StageResult> ProcessAsync(PipelineContext context);
}